using FreelanceOps.Application.Abstractions.Notifications;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Domain.Notifications;
using FreelanceOps.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Infrastructure.Notifications;

public sealed class NotificationService(
    IApplicationDbContext dbContext) : INotificationService
{
    public async Task CreateAsync(
        Guid workspaceId,
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? relatedEntityType,
        Guid? relatedEntityId,
        string? deduplicationKey,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(deduplicationKey))
        {
            var notificationExists = await dbContext.Notifications
                .AnyAsync(
                    notification => notification.DeduplicationKey == deduplicationKey,
                    cancellationToken);

            if (notificationExists)
            {
                return;
            }
        }

        dbContext.Notifications.Add(
            new Notification(
                workspaceId,
                userId,
                type,
                title,
                message,
                relatedEntityType,
                relatedEntityId,
                deduplicationKey));
    }

    public async Task CreateForWorkspaceRolesAsync(
        Guid workspaceId,
        IReadOnlyCollection<WorkspaceRole> roles,
        NotificationType type,
        string title,
        string message,
        string? relatedEntityType,
        Guid? relatedEntityId,
        string deduplicationKeyPrefix,
        CancellationToken cancellationToken)
    {
        var userIds = await dbContext.WorkspaceMembers
            .AsNoTracking()
            .Where(member =>
                member.WorkspaceId == workspaceId &&
                member.IsActive &&
                roles.Contains(member.Role))
            .Select(member => member.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var userId in userIds)
        {
            await CreateAsync(
                workspaceId,
                userId,
                type,
                title,
                message,
                relatedEntityType,
                relatedEntityId,
                $"{deduplicationKeyPrefix}:{userId}",
                cancellationToken);
        }
    }
}
