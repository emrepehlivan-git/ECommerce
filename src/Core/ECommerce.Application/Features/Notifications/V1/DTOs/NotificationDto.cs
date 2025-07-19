namespace ECommerce.Application.Features.Notifications.V1.DTOs;

public record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    string Type,
    Guid? UserId,
    DateTime CreatedAt,
    bool IsRead,
    Dictionary<string, object>? Data);