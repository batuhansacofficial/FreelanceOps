using FreelanceOps.Domain.Projects;

namespace FreelanceOps.Application.ProjectTasks.GetProjectTasks;

public sealed record GetProjectTasksQuery(
    Guid WorkspaceId,
    Guid ProjectId,
    int Page = 1,
    int PageSize = 20,
    ProjectTaskStatus? Status = null,
    Guid? AssignedToUserId = null);
