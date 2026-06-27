namespace FreelanceOps.Application.Proposals.AcceptProposal;

public sealed record AcceptProposalCommand(
    Guid WorkspaceId,
    Guid ProposalId);
