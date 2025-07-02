using ECommerce.Application.Repositories;
using ECommerce.Domain.Events.Stock;
using MediatR;

namespace ECommerce.Application.Features.Stock.V1.EventHandlers;

public class StockNotReservedEventHandler(
    IStockRepository stockRepository) : INotificationHandler<StockNotReservedEvent>
{
    public async Task Handle(StockNotReservedEvent notification, CancellationToken cancellationToken)
    {
        await stockRepository.ReleaseStockAsync(notification.ProductId, notification.RequestedQuantity, cancellationToken);
    }
}
