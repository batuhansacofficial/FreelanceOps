using FreelanceOps.Domain.Proposals;

namespace FreelanceOps.Application.Proposals.GetProposals;

public sealed record GetProposalsQuery(
    Guid WorkspaceId,
    int Page,
    int PageSize,
    ProposalStatus? Status,
    Guid? ClientId,
    string? Search);
