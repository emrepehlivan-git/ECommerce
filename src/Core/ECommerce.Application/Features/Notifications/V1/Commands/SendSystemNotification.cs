using ECommerce.Application.Services;
using ECommerce.Domain.ValueObjects;
using MediatR;

namespace ECommerce.Application.Features.Notifications.V1.Commands;

public record SendSystemNotificationCommand(
    string Title,
    string Message,
    SystemNotificationType Type,
    Guid? TargetUserId = null,
    string? TargetGroup = null) : IRequest;

public class SendSystemNotificationCommandHandler : IRequestHandler<SendSystemNotificationCommand>
{
    private readonly INotificationService _notificationService;

    public SendSystemNotificationCommandHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(SendSystemNotificationCommand request, CancellationToken cancellationToken)
    {
        var notificationType = request.Type switch
        {
            SystemNotificationType.OrderCreated => "order_created",
            SystemNotificationType.OrderStatusChanged => "order_status_changed",
            SystemNotificationType.LowStock => "low_stock",
            SystemNotificationType.OutOfStock => "out_of_stock",
            SystemNotificationType.UserRegistered => "user_registered",
            SystemNotificationType.SystemAlert => "system_alert",
            SystemNotificationType.Announcement => "announcement",
            _ => "system_notification"
        };

        var content = new NotificationContent(
            request.Title,
            request.Message,
            notificationType,
            new Dictionary<string, object>
            {
                ["timestamp"] = DateTime.UtcNow,
                ["notificationType"] = request.Type.ToString(),
                ["priority"] = GetPriority(request.Type)
            });

        if (request.TargetUserId.HasValue)
        {
            await _notificationService.SendToUserAsync(request.TargetUserId.Value, content);
        }
        else if (!string.IsNullOrEmpty(request.TargetGroup))
        {
            await _notificationService.SendToGroupAsync(request.TargetGroup, content);
        }
        else
        {
            await _notificationService.SendToAllAsync(content);
        }
    }

    private static string GetPriority(SystemNotificationType type)
    {
        return type switch
        {
            SystemNotificationType.SystemAlert => "high",
            SystemNotificationType.OutOfStock => "high",
            SystemNotificationType.OrderCreated => "medium",
            SystemNotificationType.LowStock => "medium",
            _ => "low"
        };
    }
}

public enum SystemNotificationType
{
    OrderCreated,
    OrderStatusChanged,
    LowStock,
    OutOfStock,
    UserRegistered,
    SystemAlert,
    Announcement
}