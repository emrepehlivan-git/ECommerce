using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.Specifications;

namespace ECommerce.Application.Features.Carts.V1.Specifications;

public sealed class CartItemOrderableSpecification : BaseSpecification<Product>
{
    public CartItemOrderableSpecification(Guid productId, int quantity)
    {
        Criteria = p => p.Id == productId && p.IsActive && p.Stock.Quantity >= quantity;
        AddInclude(p => p.Stock);
    }
}