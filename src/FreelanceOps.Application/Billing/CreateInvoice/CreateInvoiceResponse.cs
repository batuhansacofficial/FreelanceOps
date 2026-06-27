using FreelanceOps.Domain.Billing;

namespace FreelanceOps.Application.Billing.CreateInvoice;

public sealed record CreateInvoiceResponse(
    Guid Id,
    Guid WorkspaceId,
    Guid ClientId,
    Guid? ProjectId,
    string InvoiceNumber,
    InvoiceStatus Status,
    DateOnly IssueDate,
    DateOnly DueDate,
    string Currency,
    decimal SubtotalAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal BalanceDue);
