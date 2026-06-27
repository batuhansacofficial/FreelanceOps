namespace FreelanceOps.Application.Projects.CreateProject;

public sealed record CreateProjectCommand(
    Guid WorkspaceId,
    Guid ClientId,
    string Name,
    string? Description,
    DateOnly? StartDate,
    DateOnly? Deadline);
