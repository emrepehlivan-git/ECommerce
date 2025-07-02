using ECommerce.Application.Repositories;
using ECommerce.Domain.Events.Stock;
using MediatR;

namespace ECommerce.Application.Features.Stock.V1.EventHandlers;

public sealed class StockReservedEventHandler(IStockRepository stockRepository) : INotificationHandler<StockReservedEvent>
{
    public async Task Handle(StockReservedEvent notification, CancellationToken cancellationToken)
    {
        await stockRepository.ReserveStockAsync(notification.ProductId, notification.Quantity, cancellationToken);
    }
}