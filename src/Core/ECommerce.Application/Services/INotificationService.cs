using ECommerce.Domain.ValueObjects;

namespace ECommerce.Application.Services;

public interface INotificationService
{
    Task SendToUserAsync(Guid userId, NotificationContent content);
    Task SendToAllAsync(NotificationContent content);
    Task SendToGroupAsync(string groupName, NotificationContent content);
    Task AddUserToGroupAsync(Guid userId, string groupName);
    Task RemoveUserFromGroupAsync(Guid userId, string groupName);
}