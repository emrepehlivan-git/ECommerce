
using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.Repositories;

namespace ECommerce.Application.Repositories;

public interface IStockRepository : IRepository<ProductStock>
{
    Task ReserveStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task ReleaseStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
}
