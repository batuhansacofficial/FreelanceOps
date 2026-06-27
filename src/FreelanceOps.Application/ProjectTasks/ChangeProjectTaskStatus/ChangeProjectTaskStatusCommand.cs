using FreelanceOps.Domain.Projects;

namespace FreelanceOps.Application.ProjectTasks.ChangeProjectTaskStatus;

public sealed record ChangeProjectTaskStatusCommand(
    Guid WorkspaceId,
    Guid TaskId,
    ProjectTaskStatus Status);
