namespace FreelanceOps.Application.Proposals.RejectProposal;

public sealed record RejectProposalCommand(
    Guid WorkspaceId,
    Guid ProposalId);
