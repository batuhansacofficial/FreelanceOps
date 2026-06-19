using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Billing.UpdateInvoice;

public sealed class UpdateInvoiceHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<UpdateInvoiceCommand> validator)
{
    public async Task Handle(
        UpdateInvoiceCommand command,
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

        invoice.UpdateDetails(
            command.IssueDate,
            command.DueDate,
            command.Currency,
            command.Notes);

        var items = command.Items
            .Select(item => new InvoiceItem(
                invoice.Id,
                item.Description,
                item.Quantity,
                item.UnitPrice,
                item.TaxRate))
            .ToArray();

        invoice.ReplaceItems(items);
        dbContext.InvoiceItems.AddRange(items);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
