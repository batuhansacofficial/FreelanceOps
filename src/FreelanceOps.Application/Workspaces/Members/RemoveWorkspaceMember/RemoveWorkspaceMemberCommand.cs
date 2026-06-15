namespace FreelanceOps.Application.Workspaces.Members.RemoveWorkspaceMember;

public sealed record RemoveWorkspaceMemberCommand(Guid WorkspaceId, Guid MemberId);
