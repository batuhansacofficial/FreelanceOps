using FreelanceOps.Domain.Projects;

namespace FreelanceOps.Application.Projects.ChangeProjectStatus;

public sealed record ChangeProjectStatusCommand(
    Guid WorkspaceId,
    Guid ProjectId,
    ProjectStatus Status);
