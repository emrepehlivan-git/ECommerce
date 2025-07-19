using ECommerce.Application.Services;
using ECommerce.Domain.Events.Stock;
using ECommerce.Domain.ValueObjects;
using MediatR;

namespace ECommerce.Application.Features.Notifications.V1.EventHandlers;

public class StockNotificationHandler(INotificationService notificationService) : 
    INotificationHandler<StockReservedEvent>,
    INotificationHandler<StockNotReservedEvent>
{
    public async Task Handle(StockReservedEvent notification, CancellationToken cancellationToken)
    {
        var content = new NotificationContent(
            "Stock Reserved",
            $"Stock reserved for product: {notification.Quantity} units",
            "stock_reserved",
            new Dictionary<string, object>
            {
                ["productId"] = notification.ProductId,
                ["quantity"] = notification.Quantity
            });

        await notificationService.SendToGroupAsync("administrators", content);
    }

    public async Task Handle(StockNotReservedEvent notification, CancellationToken cancellationToken)
    {
        var content = new NotificationContent(
            "Stock Reservation Failed",
            $"Failed to reserve stock for product: {notification.RequestedQuantity} units",
            "stock_reservation_failed",
            new Dictionary<string, object>
            {
                ["productId"] = notification.ProductId,
                ["requestedQuantity"] = notification.RequestedQuantity
            });

        await notificationService.SendToGroupAsync("administrators", content);
    }
}