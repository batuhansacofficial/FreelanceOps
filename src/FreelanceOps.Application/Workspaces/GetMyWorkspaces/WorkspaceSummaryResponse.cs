namespace FreelanceOps.Application.Workspaces.GetMyWorkspaces;

public sealed record WorkspaceSummaryResponse(
    Guid Id,
    string Name,
    string Slug,
    string Role,
    DateTime CreatedAtUtc);
