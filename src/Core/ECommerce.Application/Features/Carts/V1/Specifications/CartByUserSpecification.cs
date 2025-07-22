using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.Specifications;

namespace ECommerce.Application.Features.Carts.V1.Specifications;

public sealed class CartByUserSpecification : BaseSpecification<Cart>
{
    public CartByUserSpecification(Guid userId)
    {
        Criteria = c => c.UserId == userId;
        AddInclude("Items.Product");
    }
}