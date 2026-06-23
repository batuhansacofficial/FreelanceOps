namespace FreelanceOps.Application.Proposals.SendProposal;

public sealed record SendProposalCommand(
    Guid WorkspaceId,
    Guid ProposalId);
