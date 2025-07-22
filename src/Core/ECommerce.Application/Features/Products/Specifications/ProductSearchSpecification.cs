using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.Specifications;

namespace ECommerce.Application.Features.Products.Specifications;

public sealed class ProductSearchSpecification : BaseSpecification<Product>
{
    public ProductSearchSpecification(string search)
    {
        if (!string.IsNullOrWhiteSpace(search))
            Criteria = p => p.Name.ToLower().Contains(search.ToLower());

        AddInclude(p => p.Images);
        AddInclude(p => p.Stock);
        ApplyOrderBy(p => p.Name);
    }
}