using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.Specifications;

namespace ECommerce.Application.Features.Products.Specifications;

public sealed class ProductByCategorySpecification : BaseSpecification<Product>
{
    public ProductByCategorySpecification(Guid categoryId)
    {
        Criteria = p => p.CategoryId == categoryId;
        AddInclude(p => p.Images);
        AddInclude(p => p.Stock);
        ApplyOrderBy(p => p.Name);
    }
}