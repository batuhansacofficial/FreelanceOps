using FreelanceOps.Domain.Proposals;

namespace FreelanceOps.Application.Proposals.GetProposals;

public sealed record ProposalSummaryResponse(
    Guid Id,
    Guid WorkspaceId,
    Guid ClientId,
    Guid? ConvertedProjectId,
    string ProposalNumber,
    string Title,
    ProposalStatus Status,
    DateOnly ValidUntil,
    string Currency,
    decimal TotalAmount,
    DateTime CreatedAtUtc);
