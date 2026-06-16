using FreelanceOps.Domain.Projects;

namespace FreelanceOps.Application.ProjectTasks.UpdateProjectTask;

public sealed record UpdateProjectTaskCommand(
    Guid WorkspaceId,
    Guid TaskId,
    string Title,
    string? Description,
    ProjectTaskPriority Priority,
    DateOnly? DueDate,
    Guid? AssignedToUserId);
