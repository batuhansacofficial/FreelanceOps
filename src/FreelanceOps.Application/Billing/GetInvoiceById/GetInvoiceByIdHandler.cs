using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Billing.GetInvoiceById;

public sealed class GetInvoiceByIdHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService)
{
    public async Task<InvoiceDetailResponse> Handle(
        GetInvoiceByIdQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.RequireUserId();

        await BillingGuard.EnsureManagerAsync(
            dbContext,
            workspaceAuthorizationService,
            userId,
            query.WorkspaceId,
            cancellationToken);

        var invoice = await dbContext.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Items)
            .Include(invoice => invoice.Payments)
            .FirstOrDefaultAsync(
                invoice =>
                    invoice.Id == query.InvoiceId &&
                    invoice.WorkspaceId == query.WorkspaceId &&
                    !invoice.IsDeleted,
                cancellationToken);

        if (invoice is null)
        {
            throw new NotFoundException("Invoice was not found.");
        }

        return new InvoiceDetailResponse(
            invoice.Id,
            invoice.WorkspaceId,
            invoice.ClientId,
            invoice.ProjectId,
            invoice.InvoiceNumber,
            invoice.Status,
            invoice.IssueDate,
            invoice.DueDate,
            invoice.Currency,
            invoice.Notes,
            invoice.SubtotalAmount,
            invoice.TaxAmount,
            invoice.TotalAmount,
            invoice.PaidAmount,
            invoice.BalanceDue,
            invoice.IsOverdue(DateOnly.FromDateTime(DateTime.UtcNow)),
            invoice.Items
                .Select(item => new InvoiceItemResponse(
                    item.Id,
                    item.Description,
                    item.Quantity,
                    item.UnitPrice,
                    item.TaxRate,
                    item.SubtotalAmount,
                    item.TaxAmount,
                    item.TotalAmount))
                .ToArray(),
            invoice.Payments
                .OrderByDescending(payment => payment.PaidAt)
                .Select(payment => new InvoicePaymentResponse(
                    payment.Id,
                    payment.Amount,
                    payment.Method,
                    payment.Reference,
                    payment.PaidAt,
                    payment.CreatedAtUtc))
                .ToArray(),
            invoice.CreatedAtUtc,
            invoice.UpdatedAtUtc);
    }
}
