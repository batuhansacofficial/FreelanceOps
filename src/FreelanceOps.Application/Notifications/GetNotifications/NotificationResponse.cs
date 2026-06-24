using FreelanceOps.Domain.Notifications;

namespace FreelanceOps.Application.Notifications.GetNotifications;

public sealed record NotificationResponse(
    Guid Id,
    Guid WorkspaceId,
    Guid UserId,
    NotificationType Type,
    string Title,
    string Message,
    string? RelatedEntityType,
    Guid? RelatedEntityId,
    bool IsRead,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc);
