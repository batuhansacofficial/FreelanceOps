using FreelanceOps.Domain.Billing;

namespace FreelanceOps.Application.Billing.GetInvoices;

public sealed record InvoiceSummaryResponse(
    Guid Id,
    Guid WorkspaceId,
    Guid ClientId,
    Guid? ProjectId,
    string InvoiceNumber,
    InvoiceStatus Status,
    DateOnly IssueDate,
    DateOnly DueDate,
    string Currency,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal BalanceDue,
    bool IsOverdue,
    DateTime CreatedAtUtc);
