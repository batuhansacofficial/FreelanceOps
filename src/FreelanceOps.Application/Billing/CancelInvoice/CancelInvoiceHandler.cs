using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Billing.CancelInvoice;

public sealed class CancelInvoiceHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService)
{
    public async Task Handle(
        CancelInvoiceCommand command,
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

        invoice.Cancel();

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
