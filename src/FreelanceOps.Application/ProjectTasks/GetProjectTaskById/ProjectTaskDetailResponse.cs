using FreelanceOps.Domain.Projects;

namespace FreelanceOps.Application.ProjectTasks.GetProjectTaskById;

public sealed record ProjectTaskDetailResponse(
    Guid Id,
    Guid WorkspaceId,
    Guid ProjectId,
    string Title,
    string? Description,
    ProjectTaskStatus Status,
    ProjectTaskPriority Priority,
    DateOnly? DueDate,
    Guid? AssignedToUserId,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
