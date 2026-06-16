namespace FreelanceOps.Application.ProjectTasks.GetProjectTaskById;

public sealed record GetProjectTaskByIdQuery(
    Guid WorkspaceId,
    Guid TaskId);
