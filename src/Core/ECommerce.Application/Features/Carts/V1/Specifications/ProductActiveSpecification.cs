using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.Specifications;

namespace ECommerce.Application.Features.Carts.V1.Specifications;

public sealed class ProductActiveSpecification : BaseSpecification<Product>
{
    public ProductActiveSpecification(Guid productId)
    {
        Criteria = p => p.Id == productId && p.IsActive;
        AddInclude(p => p.Stock);
    }
}