using ECommerce.Application.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Persistence.Contexts;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Persistence.Repositories;

public sealed class OrderRepository(ApplicationDbContext context) : BaseRepository<Order>(context), IOrderRepository, IScopedDependency
{
    public async Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Query(o => o.UserId == userId, isTracking: false, include: o => o.Include(o => o.Items))
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }
}