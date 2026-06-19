namespace FreelanceOps.Application.Billing.GetInvoiceById;

public sealed record GetInvoiceByIdQuery(
    Guid WorkspaceId,
    Guid InvoiceId);
