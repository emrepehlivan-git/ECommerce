using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.Specifications;

namespace ECommerce.Application.Features.UserAddresses.V1.Specifications;

public sealed class UserDefaultAddressSpecification : BaseSpecification<UserAddress>
{
    public UserDefaultAddressSpecification(Guid userId)
    {
        Criteria = x => x.UserId == userId && x.IsDefault && x.IsActive;
    }
}