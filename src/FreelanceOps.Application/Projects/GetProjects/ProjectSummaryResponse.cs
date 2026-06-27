using FreelanceOps.Domain.Projects;

namespace FreelanceOps.Application.Projects.GetProjects;

public sealed record ProjectSummaryResponse(
    Guid Id,
    Guid WorkspaceId,
    Guid ClientId,
    string Name,
    ProjectStatus Status,
    DateOnly? StartDate,
    DateOnly? Deadline,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
