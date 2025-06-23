using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.Repositories;

namespace ECommerce.Application.Repositories;

public interface ICartItemRepository : IRepository<CartItem>
{
    Task<CartItem?> GetByCartIdAndProductIdAsync(Guid cartId, Guid productId, CancellationToken cancellationToken = default);
    Task<List<CartItem>> GetByCartIdAsync(Guid cartId, CancellationToken cancellationToken = default);
} 