namespace ECommerce.Domain.ValueObjects;

public record NotificationContent(
    string Title,
    string Message,
    string Type,
    Dictionary<string, object>? Data = null);