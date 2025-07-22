using ECommerce.Application.Features.Categories.Specifications;
using ECommerce.Application.Repositories;
using ECommerce.SharedKernel.DependencyInjection;

namespace ECommerce.Application.Features.Categories.V1;

public sealed class CategoryBusinessRules(ICategoryRepository categoryRepository) : IScopedDependency
{
    public async Task<bool> CheckIfCategoryExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var spec = new CategoryNameUniqueSpecification(name, excludeId);
        return await categoryRepository.AnyAsync(spec, cancellationToken);
    }
}
