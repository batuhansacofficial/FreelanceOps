using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Workspaces.DeleteWorkspace;

public sealed class DeleteWorkspaceHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService)
{
    public async Task Handle(DeleteWorkspaceCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.RequireUserId();
        var workspace = await dbContext.Workspaces
            .FirstOrDefaultAsync(
                workspace => workspace.Id == command.WorkspaceId && !workspace.IsDeleted,
                cancellationToken);

        if (workspace is null)
        {
            throw new NotFoundException("Workspace was not found.");
        }

        await workspaceAuthorizationService.EnsureAnyRoleAsync(
            userId,
            command.WorkspaceId,
            WorkspaceRoles.Owners,
            cancellationToken);

        workspace.SoftDelete();

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
