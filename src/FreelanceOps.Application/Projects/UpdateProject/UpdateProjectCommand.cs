namespace FreelanceOps.Application.Projects.UpdateProject;

public sealed record UpdateProjectCommand(
    Guid WorkspaceId,
    Guid ProjectId,
    string Name,
    string? Description,
    DateOnly? StartDate,
    DateOnly? Deadline);
