using FreelanceOps.Application.Proposals;

namespace FreelanceOps.Application.Proposals.CreateProposal;

public sealed record CreateProposalCommand(
    Guid WorkspaceId,
    Guid ClientId,
    string Title,
    string Scope,
    DateOnly ValidUntil,
    string Currency,
    IReadOnlyCollection<ProposalItemInput> Items);
