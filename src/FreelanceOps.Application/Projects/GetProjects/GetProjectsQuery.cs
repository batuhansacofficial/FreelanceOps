using FreelanceOps.Domain.Projects;

namespace FreelanceOps.Application.Projects.GetProjects;

public sealed record GetProjectsQuery(
    Guid WorkspaceId,
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    ProjectStatus? Status = null,
    Guid? ClientId = null);
