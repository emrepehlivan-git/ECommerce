using ECommerce.Application.Repositories;
using ECommerce.SharedKernel;

namespace ECommerce.Application.Features.Categories;

public sealed class CategoryBusinessRules(ICategoryRepository categoryRepository) : IScopedDependency
{
    public async Task<bool> CheckIfCategoryExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await categoryRepository.AnyAsync(x => x.Name.Trim().ToLower() == name.ToLower() && (excludeId == null || x.Id != excludeId), cancellationToken);
    }
}
