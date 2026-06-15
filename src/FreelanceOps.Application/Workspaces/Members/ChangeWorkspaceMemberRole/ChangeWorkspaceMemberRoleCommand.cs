using FreelanceOps.Domain.Workspaces;

namespace FreelanceOps.Application.Workspaces.Members.ChangeWorkspaceMemberRole;

public sealed record ChangeWorkspaceMemberRoleCommand(
    Guid WorkspaceId,
    Guid MemberId,
    WorkspaceRole Role);
