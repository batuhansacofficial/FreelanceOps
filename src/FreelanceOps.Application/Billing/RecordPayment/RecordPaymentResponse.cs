using FreelanceOps.Domain.Billing;

namespace FreelanceOps.Application.Billing.RecordPayment;

public sealed record RecordPaymentResponse(
    Guid Id,
    Guid InvoiceId,
    decimal Amount,
    PaymentMethod Method,
    string? Reference,
    DateOnly PaidAt,
    DateTime CreatedAtUtc,
    decimal InvoicePaidAmount,
    decimal InvoiceBalanceDue,
    InvoiceStatus InvoiceStatus);
