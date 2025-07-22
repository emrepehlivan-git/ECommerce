using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.Specifications;

namespace ECommerce.Application.Features.UserAddresses.V1.Specifications;

public sealed class UserAddressesByUserSpecification : BaseSpecification<UserAddress>
{
    public UserAddressesByUserSpecification(Guid userId, bool activeOnly = true)
    {
        if (activeOnly)
            Criteria = x => x.UserId == userId && x.IsActive;
        else
            Criteria = x => x.UserId == userId;

        ApplyOrderByDescending(x => x.IsDefault);
    }
}