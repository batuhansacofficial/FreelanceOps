namespace FreelanceOps.Application.Billing.SendInvoice;

public sealed record SendInvoiceCommand(
    Guid WorkspaceId,
    Guid InvoiceId);
