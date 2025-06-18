using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.Repositories;

namespace ECommerce.Application.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<bool> HasProductsAsync(Guid id, CancellationToken cancellationToken = default);
}