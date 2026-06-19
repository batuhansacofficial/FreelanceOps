namespace FreelanceOps.Application.Billing.GetInvoicePayments;

public sealed record GetInvoicePaymentsQuery(
    Guid WorkspaceId,
    Guid InvoiceId);
