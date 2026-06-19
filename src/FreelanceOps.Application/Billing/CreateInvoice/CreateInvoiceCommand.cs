namespace FreelanceOps.Application.Billing.CreateInvoice;

public sealed record CreateInvoiceCommand(
    Guid WorkspaceId,
    Guid ClientId,
    Guid? ProjectId,
    DateOnly IssueDate,
    DateOnly DueDate,
    string Currency,
    string? Notes,
    IReadOnlyCollection<InvoiceItemInput> Items);
