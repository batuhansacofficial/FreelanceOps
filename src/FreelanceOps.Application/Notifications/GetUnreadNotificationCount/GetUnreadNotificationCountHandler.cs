using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Notifications.GetUnreadNotificationCount;

public sealed class GetUnreadNotificationCountHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService)
{
    public async Task<UnreadNotificationCountResponse> Handle(
        GetUnreadNotificationCountQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.RequireUserId();

        await workspaceAuthorizationService.EnsureMemberAsync(
            userId,
            query.WorkspaceId,
            cancellationToken);

        var count = await dbContext.Notifications
            .CountAsync(
                notification =>
                    notification.WorkspaceId == query.WorkspaceId &&
                    notification.UserId == userId &&
                    !notification.IsRead,
                cancellationToken);

        return new UnreadNotificationCountResponse(count);
    }
}
