namespace FreelanceOps.Application.Workspaces.CreateWorkspace;

public sealed record CreateWorkspaceResponse(
    Guid Id,
    string Name,
    string Slug,
    string Role,
    DateTime CreatedAtUtc);
