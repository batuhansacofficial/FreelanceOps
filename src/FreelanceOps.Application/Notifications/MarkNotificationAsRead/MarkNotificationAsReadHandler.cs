using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Notifications.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService)
{
    public async Task Handle(
        MarkNotificationAsReadCommand command,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.RequireUserId();

        await workspaceAuthorizationService.EnsureMemberAsync(
            userId,
            command.WorkspaceId,
            cancellationToken);

        var notification = await dbContext.Notifications
            .FirstOrDefaultAsync(
                notification =>
                    notification.Id == command.NotificationId &&
                    notification.WorkspaceId == command.WorkspaceId &&
                    notification.UserId == userId,
                cancellationToken);

        if (notification is null)
        {
            throw new NotFoundException("Notification was not found.");
        }

        notification.MarkAsRead();

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
