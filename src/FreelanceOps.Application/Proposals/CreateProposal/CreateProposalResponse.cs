using FreelanceOps.Domain.Proposals;

namespace FreelanceOps.Application.Proposals.CreateProposal;

public sealed record CreateProposalResponse(
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
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
