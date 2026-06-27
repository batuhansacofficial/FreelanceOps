using FreelanceOps.Application.Proposals;

namespace FreelanceOps.Application.Proposals.UpdateProposal;

public sealed record UpdateProposalCommand(
    Guid WorkspaceId,
    Guid ProposalId,
    string Title,
    string Scope,
    DateOnly ValidUntil,
    string Currency,
    IReadOnlyCollection<ProposalItemInput> Items);
