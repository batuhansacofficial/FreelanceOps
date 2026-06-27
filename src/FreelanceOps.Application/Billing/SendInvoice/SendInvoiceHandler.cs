using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Notifications;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Billing.SendInvoice;

public sealed class SendInvoiceHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    INotificationService notificationService)
{
    public async Task Handle(
        SendInvoiceCommand command,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.RequireUserId();

        await BillingGuard.EnsureManagerAsync(
            dbContext,
            workspaceAuthorizationService,
            userId,
            command.WorkspaceId,
            cancellationToken);

        var invoice = await dbContext.Invoices
            .Include(invoice => invoice.Items)
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

        invoice.MarkAsSent();

        await notificationService.CreateForWorkspaceRolesAsync(
            invoice.WorkspaceId,
            WorkspaceRoles.Managers,
            NotificationType.InvoiceSent,
            "Invoice sent",
            $"Invoice {invoice.InvoiceNumber} was sent.",
            "Invoice",
            invoice.Id,
            $"invoice-sent:{invoice.Id}",
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
