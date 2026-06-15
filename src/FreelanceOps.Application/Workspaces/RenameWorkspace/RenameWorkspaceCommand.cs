namespace FreelanceOps.Application.Workspaces.RenameWorkspace;

public sealed record RenameWorkspaceCommand(Guid WorkspaceId, string Name);
