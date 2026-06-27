using FreelanceOps.Domain.Projects;

namespace FreelanceOps.Application.Proposals.ConvertProposalToProject;

public sealed record ConvertProposalToProjectResponse(
    Guid ProjectId,
    Guid ProposalId,
    string Name,
    ProjectStatus Status);
