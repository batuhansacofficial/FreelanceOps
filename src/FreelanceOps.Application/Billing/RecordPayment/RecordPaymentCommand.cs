using FreelanceOps.Domain.Billing;

namespace FreelanceOps.Application.Billing.RecordPayment;

public sealed record RecordPaymentCommand(
    Guid WorkspaceId,
    Guid InvoiceId,
    decimal Amount,
    PaymentMethod Method,
    string? Reference,
    DateOnly PaidAt);
