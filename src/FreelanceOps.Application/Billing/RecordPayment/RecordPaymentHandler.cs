using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Notifications;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Domain.Billing;
using FreelanceOps.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Billing.RecordPayment;

public sealed class RecordPaymentHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    INotificationService notificationService,
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

        if (invoice.Status == InvoiceStatus.Paid)
        {
            await notificationService.CreateForWorkspaceRolesAsync(
                invoice.WorkspaceId,
                WorkspaceRoles.Managers,
                NotificationType.InvoicePaid,
                "Invoice paid",
                $"Invoice {invoice.InvoiceNumber} was paid.",
                "Invoice",
                invoice.Id,
                $"invoice-paid:{invoice.Id}",
                cancellationToken);
        }

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
