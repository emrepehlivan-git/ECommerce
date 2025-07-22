using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.Specifications;

namespace ECommerce.Application.Features.UserAddresses.V1.Specifications;

public sealed class UserAddressActiveSpecification : BaseSpecification<UserAddress>
{
    public UserAddressActiveSpecification(Guid addressId, Guid userId)
    {
        Criteria = x => x.Id == addressId && x.UserId == userId && x.IsActive;
    }
}