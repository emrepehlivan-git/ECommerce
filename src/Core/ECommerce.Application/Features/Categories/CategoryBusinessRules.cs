using ECommerce.Application.Repositories;
using ECommerce.SharedKernel.DependencyInjection;

namespace ECommerce.Application.Features.Categories;

public sealed class CategoryBusinessRules(ICategoryRepository categoryRepository) : IScopedDependency
{
    public async Task<bool> CheckIfCategoryExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await categoryRepository.AnyAsync(c => c.Name.Trim().ToLower() == name.ToLower() && (excludeId == null || c.Id != excludeId), cancellationToken);
    }
}
