using ECommerce.Application.Exceptions;
using ECommerce.Application.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Persistence.Contexts;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Persistence.Repositories;

public sealed class StockRepository(ApplicationDbContext context) :
 BaseRepository<ProductStock>(context),
  IStockRepository, IScopedDependency
{
    public async Task ReserveStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        try
        {
            var stock = await Query(x => x.ProductId == productId, isTracking: true)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Stock not found for product {productId}");

            if (stock.Quantity < quantity)
                throw new BusinessException($"Insufficient stock for product {productId}");

            stock.Reserve(quantity);
            await Context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Error reserving stock for product {productId}", ex);
        }
    }

    public async Task ReleaseStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        try
        {
            var stock = await Query(x => x.ProductId == productId, isTracking: true)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Stock not found for product {productId}");

            stock.Release(quantity);
            await Context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Error releasing stock for product {productId}", ex);
        }
    }
}
