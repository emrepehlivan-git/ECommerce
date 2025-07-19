using Ardalis.Result;
using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Services;
using ECommerce.Domain.ValueObjects;
using ECommerce.SharedKernel.DependencyInjection;
using MediatR;

namespace ECommerce.Application.Features.Notifications.V1.Commands;

public record SendNotificationCommand(
    string Title,
    string Message,
    string Type,
    Guid? UserId = null,
    Dictionary<string, object>? Data = null) : IRequest<Result>;

public class SendNotificationCommandHandler(INotificationService notificationService, ILazyServiceProvider lazyServiceProvider) : BaseHandler<SendNotificationCommand, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        var content = new NotificationContent(
            request.Title,
            request.Message,
            request.Type,
            request.Data);

        if (request.UserId.HasValue)
        {
            await notificationService.SendToUserAsync(request.UserId.Value, content);
        }
        else
        {
            await notificationService.SendToAllAsync(content);
        }

        return Result.Success();
    }
}