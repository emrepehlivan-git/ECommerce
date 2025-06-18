using ECommerce.Application.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Persistence.Contexts;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Persistence.Repositories;

public sealed class UserAddressRepository(ApplicationDbContext context) : BaseRepository<UserAddress>(context), IUserAddressRepository, IScopedDependency
{
    public async Task<UserAddress?> GetDefaultAddressAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserAddress>()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.IsDefault && x.IsActive, cancellationToken);
    }

    public async Task<List<UserAddress>> GetUserAddressesAsync(Guid userId, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<UserAddress>().Where(x => x.UserId == userId);
        
        if (activeOnly)
            query = query.Where(x => x.IsActive);
            
        return await query.OrderByDescending(x => x.IsDefault)
                         .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasDefaultAddressAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserAddresses
            .AnyAsync(x => x.UserId == userId && x.IsDefault && x.IsActive, cancellationToken);
    }

    public async Task SetDefaultAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default)
    {
        var currentDefaultAddresses = await _context.UserAddresses
            .Where(x => x.UserId == userId && x.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var address in currentDefaultAddresses)
        {
            address.UnsetAsDefault();
        }

        var newDefaultAddress = await _context.UserAddresses
            .FirstOrDefaultAsync(x => x.Id == addressId && x.UserId == userId && x.IsActive, cancellationToken);

        if (newDefaultAddress is not null)
        {
            newDefaultAddress.SetAsDefault();
        }
    }

    private readonly ApplicationDbContext _context = context;
} 