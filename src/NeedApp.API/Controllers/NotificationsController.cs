using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeedApp.Application.Features.Notifications.Commands;
using NeedApp.Application.Features.Notifications.Queries;

namespace NeedApp.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get paginated notifications for the current user.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetNotificationsQuery(page, pageSize), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get unread notification count for the current user.
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUnreadCountQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Mark a single notification as read.
    /// </summary>
    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new MarkNotificationReadCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Mark all notifications as read.
    /// </summary>
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        await mediator.Send(new MarkAllNotificationsReadCommand(), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Mark all notifications for a specific request as read.
    /// </summary>
    [HttpPut("read-by-request/{requestId:guid}")]
    public async Task<IActionResult> MarkAsReadByRequest(Guid requestId, CancellationToken cancellationToken)
    {
        await mediator.Send(new MarkNotificationsReadByReferenceCommand(requestId), cancellationToken);
        return NoContent();
    }
}
