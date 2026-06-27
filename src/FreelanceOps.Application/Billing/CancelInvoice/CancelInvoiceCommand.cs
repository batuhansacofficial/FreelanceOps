namespace FreelanceOps.Application.Billing.CancelInvoice;

public sealed record CancelInvoiceCommand(
    Guid WorkspaceId,
    Guid InvoiceId);
