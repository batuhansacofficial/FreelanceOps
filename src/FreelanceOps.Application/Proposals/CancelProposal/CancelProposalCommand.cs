namespace FreelanceOps.Application.Proposals.CancelProposal;

public sealed record CancelProposalCommand(
    Guid WorkspaceId,
    Guid ProposalId);
