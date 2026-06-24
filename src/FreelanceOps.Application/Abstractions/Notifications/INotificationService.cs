using FreelanceOps.Domain.Notifications;
using FreelanceOps.Domain.Workspaces;

namespace FreelanceOps.Application.Abstractions.Notifications;

public interface INotificationService
{
    Task CreateAsync(
        Guid workspaceId,
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? relatedEntityType,
        Guid? relatedEntityId,
        string? deduplicationKey,
        CancellationToken cancellationToken);

    Task CreateForWorkspaceRolesAsync(
        Guid workspaceId,
        IReadOnlyCollection<WorkspaceRole> roles,
        NotificationType type,
        string title,
        string message,
        string? relatedEntityType,
        Guid? relatedEntityId,
        string deduplicationKeyPrefix,
        CancellationToken cancellationToken);
}
