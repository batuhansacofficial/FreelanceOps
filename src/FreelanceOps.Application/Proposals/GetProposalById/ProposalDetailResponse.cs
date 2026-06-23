using FreelanceOps.Domain.Proposals;

namespace FreelanceOps.Application.Proposals.GetProposalById;

public sealed record ProposalDetailResponse(
    Guid Id,
    Guid WorkspaceId,
    Guid ClientId,
    Guid? ConvertedProjectId,
    string ProposalNumber,
    string Title,
    string Scope,
    ProposalStatus Status,
    DateOnly ValidUntil,
    string Currency,
    decimal SubtotalAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    IReadOnlyCollection<ProposalItemResponse> Items,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record ProposalItemResponse(
    Guid Id,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    decimal SubtotalAmount,
    decimal TaxAmount,
    decimal TotalAmount);
