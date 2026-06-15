namespace FreelanceOps.Application.Workspaces.GetWorkspaceById;

public sealed record WorkspaceDetailResponse(
    Guid Id,
    string Name,
    string Slug,
    string Role,
    DateTime CreatedAtUtc);
