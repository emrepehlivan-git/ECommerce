using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.Specifications;

namespace ECommerce.Application.Features.Products.Specifications;

public sealed class ProductFilterSpecification : BaseSpecification<Product>
{
    public ProductFilterSpecification(Guid? categoryId, string? search)
    {
        if (categoryId.HasValue && !string.IsNullOrWhiteSpace(search))
            Criteria = p => p.CategoryId == categoryId.Value && p.Name.ToLower().Contains(search.ToLower());
        else if (categoryId.HasValue)
            Criteria = p => p.CategoryId == categoryId.Value;
        else if (!string.IsNullOrWhiteSpace(search))
            Criteria = p => p.Name.ToLower().Contains(search.ToLower());

        AddInclude(p => p.Images);
        AddInclude(p => p.Stock);
        AddInclude(p => p.Category);
    }
}