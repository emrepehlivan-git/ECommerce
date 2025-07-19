using ECommerce.Application.Services;
using ECommerce.Domain.ValueObjects;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.Infrastructure.Services;

public class SignalRNotificationService(IHubContext<NotificationHub> hubContext) : INotificationService, IScopedDependency
{
    private static readonly Dictionary<Guid, string> _userConnections = new();
    private static readonly Dictionary<string, HashSet<string>> _groupConnections = new();

    public async Task SendToUserAsync(Guid userId, NotificationContent content)
    {
        if (_userConnections.TryGetValue(userId, out var connectionId))
        {
            await hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", content);
        }
    }

    public async Task SendToAllAsync(NotificationContent content)
    {
        await hubContext.Clients.All.SendAsync("ReceiveNotification", content);
    }

    public async Task SendToGroupAsync(string groupName, NotificationContent content)
    {
        await hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", content);
    }

    public async Task AddUserToGroupAsync(Guid userId, string groupName)
    {
        if (_userConnections.TryGetValue(userId, out var connectionId))
        {
            await hubContext.Groups.AddToGroupAsync(connectionId, groupName);
            
            if (!_groupConnections.ContainsKey(groupName))
                _groupConnections[groupName] = new HashSet<string>();
            
            _groupConnections[groupName].Add(connectionId);
        }
    }

    public async Task RemoveUserFromGroupAsync(Guid userId, string groupName)
    {
        if (_userConnections.TryGetValue(userId, out var connectionId))
        {
            await hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);
            
            if (_groupConnections.TryGetValue(groupName, out HashSet<string>? value))
            {
                value.Remove(connectionId);
                if (value.Count == 0)
                    _groupConnections.Remove(groupName);
            }
        }
    }

    public static void AddUserConnection(Guid userId, string connectionId)
    {
        _userConnections[userId] = connectionId;
    }

    public static void RemoveUserConnection(Guid userId)
    {
        if (_userConnections.TryGetValue(userId, out var connectionId))
        {
            _userConnections.Remove(userId);
            
            foreach (var group in _groupConnections.Values)
            {
                group.Remove(connectionId);
            }
        }
    }
}

public class NotificationHub : Hub
{
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}