using FreelanceOps.Domain.Workspaces;

namespace FreelanceOps.Application.Workspaces;

internal static class WorkspaceRoles
{
    public static readonly IReadOnlyCollection<WorkspaceRole> Owners = [WorkspaceRole.Owner];

    public static readonly IReadOnlyCollection<WorkspaceRole> Managers =
    [
        WorkspaceRole.Owner,
        WorkspaceRole.Admin
    ];
}
