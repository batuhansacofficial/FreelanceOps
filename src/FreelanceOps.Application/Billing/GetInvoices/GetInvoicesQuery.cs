using FreelanceOps.Domain.Billing;

namespace FreelanceOps.Application.Billing.GetInvoices;

public sealed record GetInvoicesQuery(
    Guid WorkspaceId,
    int Page = 1,
    int PageSize = 20,
    InvoiceStatus? Status = null,
    Guid? ClientId = null,
    Guid? ProjectId = null,
    string? Search = null,
    DateOnly? FromIssueDate = null,
    DateOnly? ToIssueDate = null);
