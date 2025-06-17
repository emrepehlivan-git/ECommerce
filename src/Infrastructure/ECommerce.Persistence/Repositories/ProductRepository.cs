using ECommerce.Application.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Persistence.Contexts;
using ECommerce.SharedKernel;

namespace ECommerce.Persistence.Repositories;

public sealed class ProductRepository(ApplicationDbContext context) : BaseRepository<Product>(context), IProductRepository, IScopedDependency
{
}
