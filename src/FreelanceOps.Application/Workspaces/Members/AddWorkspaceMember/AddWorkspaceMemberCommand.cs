using FreelanceOps.Domain.Workspaces;

namespace FreelanceOps.Application.Workspaces.Members.AddWorkspaceMember;

public sealed record AddWorkspaceMemberCommand(
    Guid WorkspaceId,
    string Email,
    WorkspaceRole Role);
