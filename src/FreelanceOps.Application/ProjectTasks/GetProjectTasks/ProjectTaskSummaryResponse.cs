using FreelanceOps.Domain.Projects;

namespace FreelanceOps.Application.ProjectTasks.GetProjectTasks;

public sealed record ProjectTaskSummaryResponse(
    Guid Id,
    Guid WorkspaceId,
    Guid ProjectId,
    string Title,
    ProjectTaskStatus Status,
    ProjectTaskPriority Priority,
    DateOnly? DueDate,
    Guid? AssignedToUserId,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
