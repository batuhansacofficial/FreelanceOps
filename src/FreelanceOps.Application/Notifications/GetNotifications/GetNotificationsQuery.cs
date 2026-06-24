namespace FreelanceOps.Application.Notifications.GetNotifications;

public sealed record GetNotificationsQuery(
    Guid WorkspaceId,
    int Page,
    int PageSize,
    bool? IsRead);
