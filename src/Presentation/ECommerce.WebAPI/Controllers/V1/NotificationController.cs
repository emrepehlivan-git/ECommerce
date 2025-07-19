using ECommerce.Application.Features.Notifications.V1.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebAPI.Controllers.V1;

[Authorize]
public sealed class NotificationController : BaseApiV1Controller
{
    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationCommand command)
    {
        await Mediator.Send(command);
        return Ok();
    }

    [HttpPost("send-to-user/{userId}")]
    public async Task<IActionResult> SendToUser(Guid userId, [FromBody] NotificationRequest request)
    {
        var command = new SendNotificationCommand(
            request.Title,
            request.Message,
            request.Type,
            userId,
            request.Data);

        await Mediator.Send(command);
        return Ok();
    }

    [HttpPost("send-to-all")]
    public async Task<IActionResult> SendToAll([FromBody] NotificationRequest request)
    {
        var command = new SendNotificationCommand(
            request.Title,
            request.Message,
            request.Type,
            null,
            request.Data);

        await Mediator.Send(command);
        return Ok();
    }

    [HttpPost("test")]
    public async Task<IActionResult> SendTest([FromBody] SendTestNotificationCommand command)
    {
        await Mediator.Send(command);
        return Ok(new { message = "Test notification sent successfully" });
    }
}

public record NotificationRequest(
    string Title,
    string Message,
    string Type,
    Dictionary<string, object>? Data = null);