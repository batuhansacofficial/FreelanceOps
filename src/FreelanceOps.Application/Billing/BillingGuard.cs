using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Billing;

internal static class BillingGuard
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

    public static async Task ValidateClientAndProjectAsync(
        IApplicationDbContext dbContext,
        Guid workspaceId,
        Guid clientId,
        Guid? projectId,
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

        if (!projectId.HasValue)
        {
            return;
        }

        var project = await dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(
                project =>
                    project.Id == projectId.Value &&
                    project.WorkspaceId == workspaceId &&
                    !project.IsDeleted,
                cancellationToken);

        if (project is null)
        {
            throw new NotFoundException("Project was not found.");
        }

        if (project.ClientId != clientId)
        {
            throw new DomainException("Project does not belong to the selected client.");
        }
    }
}
