using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.Specifications;

namespace ECommerce.Application.Features.Categories.Specifications;

public sealed class CategoryNameUniqueSpecification : BaseSpecification<Category>
{
    public CategoryNameUniqueSpecification(string name, Guid? excludeId = null)
    {
        Criteria = c => c.Name.Trim().ToLower() == name.ToLower() && (excludeId == null || c.Id != excludeId);
    }
}