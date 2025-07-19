using ECommerce.Application.Features.Notifications.V1.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace ECommerce.WebAPI.Controllers.V1;

[Authorize]
public sealed class NotificationController : BaseApiV1Controller
{

    [HttpPost("system")]
    public async Task<IActionResult> SendSystemNotification([FromBody] SendSystemNotificationCommand command)
    {
        await Mediator.Send(command);
        return Ok(new { message = "System notification sent successfully" });
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> SendBulkNotification([FromBody] SendBulkNotificationCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("announcement")]
    public async Task<IActionResult> SendAnnouncement([FromBody] AnnouncementRequest request)
    {
        var command = new SendSystemNotificationCommand(
            request.Title,
            request.Message,
            SystemNotificationType.Announcement,
            request.TargetUserId,
            request.TargetGroup);

        await Mediator.Send(command);
        return Ok(new { message = "Announcement sent successfully" });
    }

}

public record AnnouncementRequest(
    string Title,
    string Message,
    Guid? TargetUserId = null,
    string? TargetGroup = null);