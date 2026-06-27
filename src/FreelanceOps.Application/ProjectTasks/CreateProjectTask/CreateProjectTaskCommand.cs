using FreelanceOps.Domain.Projects;

namespace FreelanceOps.Application.ProjectTasks.CreateProjectTask;

public sealed record CreateProjectTaskCommand(
    Guid WorkspaceId,
    Guid ProjectId,
    string Title,
    string? Description,
    ProjectTaskPriority Priority,
    DateOnly? DueDate,
    Guid? AssignedToUserId);
