using FreelanceOps.Domain.Projects;

namespace FreelanceOps.Application.Projects.CreateProject;

public sealed record CreateProjectResponse(
    Guid Id,
    Guid WorkspaceId,
    Guid ClientId,
    string Name,
    string? Description,
    ProjectStatus Status,
    DateOnly? StartDate,
    DateOnly? Deadline,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
