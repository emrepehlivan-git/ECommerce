using ECommerce.Application.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Persistence.Contexts;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Persistence.Repositories;

public sealed class OrderItemRepository(ApplicationDbContext context) : BaseRepository<OrderItem>(context), IOrderItemRepository, IScopedDependency
{
    public async Task<IEnumerable<OrderItem>> GetOrderItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await Query(i => i.OrderId == orderId, isTracking: false, include: i => i.Include(i => i.Product))
            .ToListAsync(cancellationToken);
    }
}