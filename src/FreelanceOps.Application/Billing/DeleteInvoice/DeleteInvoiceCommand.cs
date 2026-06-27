namespace FreelanceOps.Application.Billing.DeleteInvoice;

public sealed record DeleteInvoiceCommand(
    Guid WorkspaceId,
    Guid InvoiceId);
