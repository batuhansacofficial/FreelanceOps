using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Proposals;

internal static class ProposalGuard
{
    public static async Task EnsureManagerAsync(
        IApplicationDbContext dbContext,
        IWorkspaceAuthorizationService workspaceAuthorizationService,
        Guid userId,
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        var workspaceExists = await dbContext.Workspaces
            .AnyAsync(
                workspace => workspace.Id == workspaceId && !workspace.IsDeleted,
                cancellationToken);

        if (!workspaceExists)
        {
            throw new NotFoundException("Workspace was not found.");
        }

        await workspaceAuthorizationService.EnsureAnyRoleAsync(
            userId,
            workspaceId,
            WorkspaceRoles.Managers,
            cancellationToken);
    }

    public static async Task EnsureActiveClientAsync(
        IApplicationDbContext dbContext,
        Guid workspaceId,
        Guid clientId,
        CancellationToken cancellationToken)
    {
        var clientExists = await dbContext.Clients
            .AnyAsync(
                client =>
                    client.Id == clientId &&
                    client.WorkspaceId == workspaceId &&
                    !client.IsDeleted,
                cancellationToken);

        if (!clientExists)
        {
            throw new NotFoundException("Client was not found.");
        }
    }
}
