using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Billing;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Domain.Billing;

namespace FreelanceOps.Application.Billing.CreateInvoice;

public sealed class CreateInvoiceHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IInvoiceNumberGenerator invoiceNumberGenerator,
    IValidator<CreateInvoiceCommand> validator)
{
    public async Task<CreateInvoiceResponse> Handle(
        CreateInvoiceCommand command,
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

        await BillingGuard.ValidateClientAndProjectAsync(
            dbContext,
            command.WorkspaceId,
            command.ClientId,
            command.ProjectId,
            cancellationToken);

        var invoiceNumber = await invoiceNumberGenerator.GenerateAsync(
            command.WorkspaceId,
            command.IssueDate,
            cancellationToken);
        var invoice = new Invoice(
            command.WorkspaceId,
            command.ClientId,
            command.ProjectId,
            invoiceNumber,
            command.IssueDate,
            command.DueDate,
            command.Currency,
            command.Notes);

        foreach (var item in command.Items)
        {
            invoice.AddItem(
                item.Description,
                item.Quantity,
                item.UnitPrice,
                item.TaxRate);
        }

        dbContext.Invoices.Add(invoice);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateInvoiceResponse(
            invoice.Id,
            invoice.WorkspaceId,
            invoice.ClientId,
            invoice.ProjectId,
            invoice.InvoiceNumber,
            invoice.Status,
            invoice.IssueDate,
            invoice.DueDate,
            invoice.Currency,
            invoice.SubtotalAmount,
            invoice.TaxAmount,
            invoice.TotalAmount,
            invoice.PaidAmount,
            invoice.BalanceDue);
    }
}
