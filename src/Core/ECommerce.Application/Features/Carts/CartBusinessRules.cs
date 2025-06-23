using ECommerce.Application.Exceptions;
using ECommerce.Application.Repositories;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Features.Carts;

public sealed class CartBusinessRules(
    ICartRepository cartRepository,
    IProductRepository productRepository)
{
    public async Task CheckCartExistsAsync(Guid cartId)
    {
        var cart = await cartRepository.GetByIdAsync(cartId);
        if (cart is null)
            throw new NotFoundException(CartConsts.ErrorMessages.CartNotFound);
    }

    public async Task CheckProductExistsAsync(Guid productId)
    {
        var product = await productRepository.GetByIdAsync(productId);
        if (product is null)
            throw new NotFoundException(CartConsts.ErrorMessages.ProductNotFound);
    }

    public void CheckProductIsActive(Product product)
    {
        if (!product.IsActive)
            throw new BusinessException(CartConsts.ErrorMessages.ProductNotActive);
    }

    public void CheckSufficientStock(Product product, int requestedQuantity)
    {
        if (!product.HasSufficientStock(requestedQuantity))
            throw new BusinessException(CartConsts.ErrorMessages.InsufficientStock);
    }

    public void CheckMaxItemsInCart(Cart cart, bool isNewItem)
    {
        if (isNewItem && cart.Items.Count >= CartConsts.MaxItemsInCart)
            throw new BusinessException(string.Format(CartConsts.ErrorMessages.MaxItemsExceeded, CartConsts.MaxItemsInCart));
    }

    public void CheckMaxQuantityPerItem(int quantity)
    {
        if (quantity > CartConsts.MaxQuantityPerItem)
            throw new BusinessException(string.Format(CartConsts.ErrorMessages.MaxQuantityExceeded, CartConsts.MaxQuantityPerItem));
    }

    public void CheckMaxTotalAmount(decimal totalAmount)
    {
        if (totalAmount > CartConsts.MaxTotalAmount)
            throw new BusinessException(string.Format(CartConsts.ErrorMessages.MaxTotalAmountExceeded, CartConsts.MaxTotalAmount));
    }
} 