namespace FreelanceOps.Application.Billing.UpdateInvoice;

public sealed record UpdateInvoiceCommand(
    Guid WorkspaceId,
    Guid InvoiceId,
    DateOnly IssueDate,
    DateOnly DueDate,
    string Currency,
    string? Notes,
    IReadOnlyCollection<InvoiceItemInput> Items);
