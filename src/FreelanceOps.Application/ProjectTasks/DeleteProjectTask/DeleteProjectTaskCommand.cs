namespace FreelanceOps.Application.ProjectTasks.DeleteProjectTask;

public sealed record DeleteProjectTaskCommand(
    Guid WorkspaceId,
    Guid TaskId);
