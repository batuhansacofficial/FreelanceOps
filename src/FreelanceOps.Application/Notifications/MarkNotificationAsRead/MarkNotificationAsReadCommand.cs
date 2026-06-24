namespace FreelanceOps.Application.Notifications.MarkNotificationAsRead;

public sealed record MarkNotificationAsReadCommand(
    Guid WorkspaceId,
    Guid NotificationId);
