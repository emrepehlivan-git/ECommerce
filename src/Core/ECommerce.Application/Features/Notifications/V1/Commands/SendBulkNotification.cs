using ECommerce.Application.Services;
using ECommerce.Domain.ValueObjects;
using MediatR;

namespace ECommerce.Application.Features.Notifications.V1.Commands;

public record SendBulkNotificationCommand(
    string Title,
    string Message,
    BulkNotificationTarget Target,
    List<Guid>? UserIds = null,
    List<string>? Groups = null) : IRequest<BulkNotificationResult>;

public class SendBulkNotificationCommandHandler : IRequestHandler<SendBulkNotificationCommand, BulkNotificationResult>
{
    private readonly INotificationService _notificationService;

    public SendBulkNotificationCommandHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<BulkNotificationResult> Handle(SendBulkNotificationCommand request, CancellationToken cancellationToken)
    {
        var content = new NotificationContent(
            request.Title,
            request.Message,
            "bulk_notification",
            new Dictionary<string, object>
            {
                ["timestamp"] = DateTime.UtcNow,
                ["target"] = request.Target.ToString(),
                ["bulk"] = true
            });

        var result = new BulkNotificationResult();

        try
        {
            switch (request.Target)
            {
                case BulkNotificationTarget.AllUsers:
                    await _notificationService.SendToAllAsync(content);
                    result.SuccessCount = 1; // We don't know exact count for broadcast
                    break;

                case BulkNotificationTarget.SpecificUsers:
                    if (request.UserIds?.Any() == true)
                    {
                        foreach (var userId in request.UserIds)
                        {
                            try
                            {
                                await _notificationService.SendToUserAsync(userId, content);
                                result.SuccessCount++;
                            }
                            catch (Exception ex)
                            {
                                result.FailedUserIds.Add(userId);
                                result.Errors.Add($"User {userId}: {ex.Message}");
                            }
                        }
                    }
                    break;

                case BulkNotificationTarget.Groups:
                    if (request.Groups?.Any() == true)
                    {
                        foreach (var group in request.Groups)
                        {
                            try
                            {
                                await _notificationService.SendToGroupAsync(group, content);
                                result.SuccessCount++;
                            }
                            catch (Exception ex)
                            {
                                result.FailedGroups.Add(group);
                                result.Errors.Add($"Group {group}: {ex.Message}");
                            }
                        }
                    }
                    break;
            }

            result.IsSuccess = result.SuccessCount > 0;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Errors.Add(ex.Message);
        }

        return result;
    }
}

public enum BulkNotificationTarget
{
    AllUsers,
    SpecificUsers,
    Groups
}

public class BulkNotificationResult
{
    public bool IsSuccess { get; set; }
    public int SuccessCount { get; set; }
    public List<Guid> FailedUserIds { get; set; } = new();
    public List<string> FailedGroups { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}