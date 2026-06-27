using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.TimeTracking.DeleteTimeEntry;

public sealed class DeleteTimeEntryHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IWorkspaceAccessService workspaceAccessService)
{
    public async Task Handle(
        DeleteTimeEntryCommand command,
        CancellationToken cancellationToken)
    {
        var currentUserId = currentUserService.RequireUserId();
        var workspaceExists = await dbContext.Workspaces
            .AnyAsync(
                workspace => workspace.Id == command.WorkspaceId && !workspace.IsDeleted,
                cancellationToken);

        if (!workspaceExists)
        {
            throw new NotFoundException("Workspace was not found.");
        }

        await workspaceAuthorizationService.EnsureMemberAsync(
            currentUserId,
            command.WorkspaceId,
            cancellationToken);

        var timeEntry = await dbContext.TimeEntries
            .FirstOrDefaultAsync(
                timeEntry =>
                    timeEntry.Id == command.TimeEntryId &&
                    timeEntry.WorkspaceId == command.WorkspaceId &&
                    !timeEntry.IsDeleted,
                cancellationToken);

        if (timeEntry is null)
        {
            throw new NotFoundException("Time entry was not found.");
        }

        var role = await workspaceAccessService.GetRoleAsync(
            currentUserId,
            command.WorkspaceId,
            cancellationToken);
        var isManager = role.HasValue && WorkspaceRoles.Managers.Contains(role.Value);

        if (!isManager && timeEntry.UserId != currentUserId)
        {
            throw new ForbiddenException("You cannot delete another user's time entry.");
        }

        timeEntry.SoftDelete();

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
