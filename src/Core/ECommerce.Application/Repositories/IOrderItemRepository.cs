using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.Repositories;

namespace ECommerce.Application.Repositories;

public interface IOrderItemRepository : IRepository<OrderItem>
{
    Task<IEnumerable<OrderItem>> GetOrderItemsAsync(Guid orderId, CancellationToken cancellationToken = default);
}