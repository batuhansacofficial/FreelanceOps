using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Workspaces.GetWorkspaceById;

public sealed class GetWorkspaceByIdHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IWorkspaceAccessService workspaceAccessService)
{
    public async Task<WorkspaceDetailResponse> Handle(
        GetWorkspaceByIdQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.RequireUserId();
        var workspace = await dbContext.Workspaces
            .AsNoTracking()
            .FirstOrDefaultAsync(
                workspace => workspace.Id == query.WorkspaceId && !workspace.IsDeleted,
                cancellationToken);

        if (workspace is null)
        {
            throw new NotFoundException("Workspace was not found.");
        }

        await workspaceAuthorizationService.EnsureMemberAsync(
            userId,
            query.WorkspaceId,
            cancellationToken);

        var role = await workspaceAccessService.GetRoleAsync(
            userId,
            query.WorkspaceId,
            cancellationToken);

        return new WorkspaceDetailResponse(
            workspace.Id,
            workspace.Name,
            workspace.Slug,
            role?.ToString() ?? string.Empty,
            workspace.CreatedAtUtc);
    }
}
