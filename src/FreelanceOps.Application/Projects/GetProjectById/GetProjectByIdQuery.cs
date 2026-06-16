namespace FreelanceOps.Application.Projects.GetProjectById;

public sealed record GetProjectByIdQuery(
    Guid WorkspaceId,
    Guid ProjectId);
