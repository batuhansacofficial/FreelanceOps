using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Billing.GetInvoicePayments;

public sealed class GetInvoicePaymentsHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService)
{
    public async Task<IReadOnlyCollection<PaymentRecordResponse>> Handle(
        GetInvoicePaymentsQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.RequireUserId();

        await BillingGuard.EnsureManagerAsync(
            dbContext,
            workspaceAuthorizationService,
            userId,
            query.WorkspaceId,
            cancellationToken);

        var invoiceExists = await dbContext.Invoices
            .AnyAsync(
                invoice =>
                    invoice.Id == query.InvoiceId &&
                    invoice.WorkspaceId == query.WorkspaceId &&
                    !invoice.IsDeleted,
                cancellationToken);

        if (!invoiceExists)
        {
            throw new NotFoundException("Invoice was not found.");
        }

        return await dbContext.PaymentRecords
            .AsNoTracking()
            .Where(payment => payment.InvoiceId == query.InvoiceId)
            .OrderByDescending(payment => payment.PaidAt)
            .ThenByDescending(payment => payment.CreatedAtUtc)
            .Select(payment => new PaymentRecordResponse(
                payment.Id,
                payment.InvoiceId,
                payment.Amount,
                payment.Method,
                payment.Reference,
                payment.PaidAt,
                payment.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
