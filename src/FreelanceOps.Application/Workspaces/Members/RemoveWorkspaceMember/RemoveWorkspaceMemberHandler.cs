using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Workspaces.Members.RemoveWorkspaceMember;

public sealed class RemoveWorkspaceMemberHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService)
{
    public async Task Handle(
        RemoveWorkspaceMemberCommand command,
        CancellationToken cancellationToken)
    {
        var requesterUserId = currentUserService.RequireUserId();
        var workspaceExists = await dbContext.Workspaces
            .AnyAsync(
                workspace => workspace.Id == command.WorkspaceId && !workspace.IsDeleted,
                cancellationToken);

        if (!workspaceExists)
        {
            throw new NotFoundException("Workspace was not found.");
        }

        await workspaceAuthorizationService.EnsureAnyRoleAsync(
            requesterUserId,
            command.WorkspaceId,
            WorkspaceRoles.Managers,
            cancellationToken);

        var member = await dbContext.WorkspaceMembers
            .FirstOrDefaultAsync(
                member =>
                    member.Id == command.MemberId &&
                    member.WorkspaceId == command.WorkspaceId &&
                    member.IsActive,
                cancellationToken);

        if (member is null)
        {
            throw new NotFoundException("Workspace member was not found.");
        }

        member.Deactivate();

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
