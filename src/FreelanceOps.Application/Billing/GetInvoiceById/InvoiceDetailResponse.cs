using FreelanceOps.Domain.Billing;

namespace FreelanceOps.Application.Billing.GetInvoiceById;

public sealed record InvoiceDetailResponse(
    Guid Id,
    Guid WorkspaceId,
    Guid ClientId,
    Guid? ProjectId,
    string InvoiceNumber,
    InvoiceStatus Status,
    DateOnly IssueDate,
    DateOnly DueDate,
    string Currency,
    string? Notes,
    decimal SubtotalAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal BalanceDue,
    bool IsOverdue,
    IReadOnlyCollection<InvoiceItemResponse> Items,
    IReadOnlyCollection<InvoicePaymentResponse> Payments,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record InvoiceItemResponse(
    Guid Id,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    decimal SubtotalAmount,
    decimal TaxAmount,
    decimal TotalAmount);

public sealed record InvoicePaymentResponse(
    Guid Id,
    decimal Amount,
    PaymentMethod Method,
    string? Reference,
    DateOnly PaidAt,
    DateTime CreatedAtUtc);
