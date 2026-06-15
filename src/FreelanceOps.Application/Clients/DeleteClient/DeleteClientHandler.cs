using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Clients.DeleteClient;

public sealed class DeleteClientHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService)
{
    public async Task Handle(
        DeleteClientCommand command,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.RequireUserId();
        var workspaceExists = await dbContext.Workspaces
            .AnyAsync(
                workspace => workspace.Id == command.WorkspaceId && !workspace.IsDeleted,
                cancellationToken);

        if (!workspaceExists)
        {
            throw new NotFoundException("Workspace was not found.");
        }

        await workspaceAuthorizationService.EnsureAnyRoleAsync(
            userId,
            command.WorkspaceId,
            WorkspaceRoles.Managers,
            cancellationToken);

        var client = await dbContext.Clients
            .FirstOrDefaultAsync(
                client =>
                    client.Id == command.ClientId &&
                    client.WorkspaceId == command.WorkspaceId &&
                    !client.IsDeleted,
                cancellationToken);

        if (client is null)
        {
            throw new NotFoundException("Client was not found.");
        }

        client.SoftDelete();

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
