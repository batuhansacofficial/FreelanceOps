namespace FreelanceOps.Application.Proposals.DeleteProposal;

public sealed record DeleteProposalCommand(
    Guid WorkspaceId,
    Guid ProposalId);
