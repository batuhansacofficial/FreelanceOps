using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Pagination;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Notifications.GetNotifications;

public sealed class GetNotificationsHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<GetNotificationsQuery> validator)
{
    public async Task<PagedResult<NotificationResponse>> Handle(
        GetNotificationsQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var userId = currentUserService.RequireUserId();

        await workspaceAuthorizationService.EnsureMemberAsync(
            userId,
            query.WorkspaceId,
            cancellationToken);

        var notificationsQuery = dbContext.Notifications
            .AsNoTracking()
            .Where(notification =>
                notification.WorkspaceId == query.WorkspaceId &&
                notification.UserId == userId);

        if (query.IsRead.HasValue)
        {
            notificationsQuery = notificationsQuery.Where(
                notification => notification.IsRead == query.IsRead.Value);
        }

        var totalCount = await notificationsQuery.CountAsync(cancellationToken);
        var notifications = await notificationsQuery
            .OrderByDescending(notification => notification.CreatedAtUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(notification => new NotificationResponse(
                notification.Id,
                notification.WorkspaceId,
                notification.UserId,
                notification.Type,
                notification.Title,
                notification.Message,
                notification.RelatedEntityType,
                notification.RelatedEntityId,
                notification.IsRead,
                notification.CreatedAtUtc,
                notification.ReadAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<NotificationResponse>(
            notifications,
            query.Page,
            query.PageSize,
            totalCount);
    }
}
