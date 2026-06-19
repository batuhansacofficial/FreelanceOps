using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Billing.RecordPayment;

public sealed class RecordPaymentHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<RecordPaymentCommand> validator)
{
    public async Task<RecordPaymentResponse> Handle(
        RecordPaymentCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var userId = currentUserService.RequireUserId();

        await BillingGuard.EnsureManagerAsync(
            dbContext,
            workspaceAuthorizationService,
            userId,
            command.WorkspaceId,
            cancellationToken);

        var invoice = await dbContext.Invoices
            .Include(invoice => invoice.Payments)
            .FirstOrDefaultAsync(
                invoice =>
                    invoice.Id == command.InvoiceId &&
                    invoice.WorkspaceId == command.WorkspaceId &&
                    !invoice.IsDeleted,
                cancellationToken);

        if (invoice is null)
        {
            throw new NotFoundException("Invoice was not found.");
        }

        var payment = invoice.RecordPayment(
            command.Amount,
            command.Method,
            command.Reference,
            command.PaidAt);

        dbContext.PaymentRecords.Add(payment);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new RecordPaymentResponse(
            payment.Id,
            payment.InvoiceId,
            payment.Amount,
            payment.Method,
            payment.Reference,
            payment.PaidAt,
            payment.CreatedAtUtc,
            invoice.PaidAmount,
            invoice.BalanceDue,
            invoice.Status);
    }
}
