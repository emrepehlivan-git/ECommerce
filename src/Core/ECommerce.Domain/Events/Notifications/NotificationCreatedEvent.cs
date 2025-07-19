using ECommerce.SharedKernel.Events;

namespace ECommerce.Domain.Events.Notifications;

public sealed record NotificationCreatedEvent(
    Guid NotificationId,
    string Title,
    string Message,
    string Type,
    Guid? UserId,
    Dictionary<string, object>? Data) : IDomainEvent;