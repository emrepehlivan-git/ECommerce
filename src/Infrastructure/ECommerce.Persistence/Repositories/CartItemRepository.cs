using ECommerce.Application.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Persistence.Contexts;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Persistence.Repositories;

public sealed class CartItemRepository(ApplicationDbContext context) : BaseRepository<CartItem>(context), ICartItemRepository, IScopedDependency
{
    public async Task<CartItem?> GetByCartIdAndProductIdAsync(Guid cartId, Guid productId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<CartItem>()
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId, cancellationToken);
    }

    public async Task<List<CartItem>> GetByCartIdAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<CartItem>()
            .Include(ci => ci.Product)
            .Where(ci => ci.CartId == cartId)
            .ToListAsync(cancellationToken);
    }
} 