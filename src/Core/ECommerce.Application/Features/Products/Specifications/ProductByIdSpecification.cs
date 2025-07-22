using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.Specifications;

namespace ECommerce.Application.Features.Products.Specifications;

public sealed class ProductByIdSpecification : BaseSpecification<Product>
{
    public ProductByIdSpecification(Guid productId)
    {
        Criteria = p => p.Id == productId;
        AddInclude(p => p.Stock);
        AddInclude(p => p.Category);
        AddInclude(p => p.Images);
    }
}