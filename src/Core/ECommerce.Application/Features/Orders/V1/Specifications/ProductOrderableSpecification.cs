using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.Specifications;

namespace ECommerce.Application.Features.Orders.V1.Specifications;

public sealed class ProductOrderableSpecification : BaseSpecification<Product>
{
    public ProductOrderableSpecification()
    {
        Criteria = p => p.IsActive;
        AddInclude(p => p.Stock);
        AddInclude(p => p.Category);
    }

    public ProductOrderableSpecification(Guid productId)
    {
        Criteria = p => p.Id == productId && p.IsActive;
        AddInclude(p => p.Stock);
        AddInclude(p => p.Category);
    }

    public ProductOrderableSpecification(List<Guid> productIds)
    {
        Criteria = p => productIds.Contains(p.Id) && p.IsActive;
        AddInclude(p => p.Stock);
        AddInclude(p => p.Category);
    }
} 