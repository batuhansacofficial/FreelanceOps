using FreelanceOps.Application.Common.Pagination;
using FreelanceOps.Application.Notifications.GetNotifications;
using FreelanceOps.Application.Notifications.GetUnreadNotificationCount;
using FreelanceOps.Application.Notifications.MarkAllNotificationsAsRead;
using FreelanceOps.Application.Notifications.MarkNotificationAsRead;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceOps.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/workspaces/{workspaceId:guid}/notifications")]
public sealed class NotificationsController(
    GetNotificationsHandler getNotificationsHandler,
    GetUnreadNotificationCountHandler getUnreadNotificationCountHandler,
    MarkNotificationAsReadHandler markNotificationAsReadHandler,
    MarkAllNotificationsAsReadHandler markAllNotificationsAsReadHandler) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<NotificationResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        Guid workspaceId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isRead = null,
        CancellationToken cancellationToken = default)
    {
        var response = await getNotificationsHandler.Handle(
            new GetNotificationsQuery(workspaceId, page, pageSize, isRead),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("unread-count")]
    [ProducesResponseType<UnreadNotificationCountResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        var response = await getUnreadNotificationCountHandler.Handle(
            new GetUnreadNotificationCountQuery(workspaceId),
            cancellationToken);

        return Ok(response);
    }

    [HttpPatch("{notificationId:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAsRead(
        Guid workspaceId,
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        await markNotificationAsReadHandler.Handle(
            new MarkNotificationAsReadCommand(workspaceId, notificationId),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllAsRead(
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        await markAllNotificationsAsReadHandler.Handle(
            new MarkAllNotificationsAsReadCommand(workspaceId),
            cancellationToken);

        return NoContent();
    }
}
