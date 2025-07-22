using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.Specifications;

namespace ECommerce.Application.Features.Users.V1.Specifications;

public sealed class UserSearchSpecification : BaseSpecification<User>
{
    public UserSearchSpecification(string? search)
    {
        if (!string.IsNullOrWhiteSpace(search))
            Criteria = u => u.FullName.FirstName.ToLower().Contains(search.ToLower()) ||
                            u.FullName.LastName.ToLower().Contains(search.ToLower()) ||
                            (string.IsNullOrWhiteSpace(u.Email) && u.Email!.ToLower().Contains(search.ToLower()));
    }
}