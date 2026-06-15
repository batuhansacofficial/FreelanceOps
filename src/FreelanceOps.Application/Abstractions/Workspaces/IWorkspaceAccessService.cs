using FreelanceOps.Domain.Workspaces;

namespace FreelanceOps.Application.Abstractions.Workspaces;

public interface IWorkspaceAccessService
{
    Task<bool> HasAccessAsync(
        Guid userId,
        Guid workspaceId,
        CancellationToken cancellationToken);

    Task<bool> HasAnyRoleAsync(
        Guid userId,
        Guid workspaceId,
        IReadOnlyCollection<WorkspaceRole> roles,
        CancellationToken cancellationToken);

    Task<WorkspaceRole?> GetRoleAsync(
        Guid userId,
        Guid workspaceId,
        CancellationToken cancellationToken);
}
