using FreelanceOps.Domain.Workspaces;

namespace FreelanceOps.Application.Abstractions.Workspaces;

public interface IWorkspaceAuthorizationService
{
    Task EnsureMemberAsync(
        Guid userId,
        Guid workspaceId,
        CancellationToken cancellationToken);

    Task EnsureAnyRoleAsync(
        Guid userId,
        Guid workspaceId,
        IReadOnlyCollection<WorkspaceRole> roles,
        CancellationToken cancellationToken);
}
