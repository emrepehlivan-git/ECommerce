using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.Repositories;

namespace ECommerce.Application.Repositories;

public interface IUserAddressRepository : IRepository<UserAddress>
{
    Task<UserAddress?> GetDefaultAddressAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<UserAddress>> GetUserAddressesAsync(Guid userId, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<bool> HasDefaultAddressAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SetDefaultAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);
} 