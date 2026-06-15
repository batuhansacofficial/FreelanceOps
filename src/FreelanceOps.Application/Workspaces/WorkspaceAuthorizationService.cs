using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Domain.Workspaces;

namespace FreelanceOps.Application.Workspaces;

public sealed class WorkspaceAuthorizationService(
    IWorkspaceAccessService workspaceAccessService) : IWorkspaceAuthorizationService
{
    public async Task EnsureMemberAsync(
        Guid userId,
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        var hasAccess = await workspaceAccessService.HasAccessAsync(
            userId,
            workspaceId,
            cancellationToken);

        if (!hasAccess)
        {
            throw new ForbiddenException("You do not have access to this workspace.");
        }
    }

    public async Task EnsureAnyRoleAsync(
        Guid userId,
        Guid workspaceId,
        IReadOnlyCollection<WorkspaceRole> roles,
        CancellationToken cancellationToken)
    {
        var hasRole = await workspaceAccessService.HasAnyRoleAsync(
            userId,
            workspaceId,
            roles,
            cancellationToken);

        if (!hasRole)
        {
            throw new ForbiddenException("You do not have permission to perform this action.");
        }
    }
}
