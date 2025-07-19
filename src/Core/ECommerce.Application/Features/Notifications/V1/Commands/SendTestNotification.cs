using ECommerce.Application.Services;
using ECommerce.Domain.ValueObjects;
using MediatR;

namespace ECommerce.Application.Features.Notifications.V1.Commands;

public record SendTestNotificationCommand(
    string Message = "Test notification from backend") : IRequest;

public class SendTestNotificationCommandHandler : IRequestHandler<SendTestNotificationCommand>
{
    private readonly INotificationService _notificationService;

    public SendTestNotificationCommandHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(SendTestNotificationCommand request, CancellationToken cancellationToken)
    {
        var content = new NotificationContent(
            "System Test",
            request.Message,
            "system_test",
            new Dictionary<string, object>
            {
                ["timestamp"] = DateTime.UtcNow,
                ["type"] = "test"
            });

        await _notificationService.SendToAllAsync(content);
    }
}