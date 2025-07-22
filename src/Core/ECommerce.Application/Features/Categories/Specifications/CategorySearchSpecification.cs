using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.Specifications;

namespace ECommerce.Application.Features.Categories.Specifications;

public sealed class CategorySearchSpecification : BaseSpecification<Category>
{
    public CategorySearchSpecification(string? search)
    {
        if (!string.IsNullOrWhiteSpace(search))
            Criteria = x => x.Name.ToLower().Contains(search.ToLower());

        ApplyOrderBy(x => x.Name);
    }
}