using FreelanceOps.Domain.Billing;

namespace FreelanceOps.Application.Billing.GetInvoicePayments;

public sealed record PaymentRecordResponse(
    Guid Id,
    Guid InvoiceId,
    decimal Amount,
    PaymentMethod Method,
    string? Reference,
    DateOnly PaidAt,
    DateTime CreatedAtUtc);
