namespace FreelanceOps.Application.Projects.DeleteProject;

public sealed record DeleteProjectCommand(
    Guid WorkspaceId,
    Guid ProjectId);
