namespace FreelanceOps.Application.Proposals.GetProposalById;

public sealed record GetProposalByIdQuery(
    Guid WorkspaceId,
    Guid ProposalId);
