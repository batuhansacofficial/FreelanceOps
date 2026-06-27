namespace FreelanceOps.Application.Proposals.ConvertProposalToProject;

public sealed record ConvertProposalToProjectCommand(
    Guid WorkspaceId,
    Guid ProposalId,
    DateOnly? StartDate,
    DateOnly? Deadline);
