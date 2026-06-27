namespace FreelanceOps.Application.Workspaces.Members;

public sealed record WorkspaceMemberResponse(
    Guid Id,
    Guid UserId,
    string Email,
    string FullName,
    string Role,
    DateTime JoinedAtUtc);
