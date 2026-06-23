using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Reports;

internal static class ReportGuard
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
}
