using ECommerce.Application.Exceptions;
using ECommerce.Application.Helpers;
using ECommerce.Application.Repositories;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Features.Carts.V1;

public sealed class CartBusinessRules(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    LocalizationHelper localizationHelper)
{
    public async Task CheckCartExistsAsync(Guid cartId)
    {
        _ = await cartRepository.GetByIdAsync(cartId) ?? throw new NotFoundException(localizationHelper[CartConsts.ErrorMessages.CartNotFound]);
    }

    public async Task CheckProductExistsAsync(Guid productId)
    {
        _ = await productRepository.GetByIdAsync(productId) ?? throw new NotFoundException(localizationHelper[CartConsts.ErrorMessages.ProductNotFound]);
    }

    public void CheckProductIsActive(Product product)
    {
        if (!product.IsActive)
            throw new BusinessException(localizationHelper[CartConsts.ErrorMessages.ProductNotActive]);
    }

    public void CheckSufficientStock(Product product, int requestedQuantity)
    {
        if (!product.HasSufficientStock(requestedQuantity))
            throw new BusinessException(localizationHelper[CartConsts.ErrorMessages.InsufficientStock]);
    }

    public void CheckMaxItemsInCart(Cart cart, bool isNewItem)
    {
        if (isNewItem && cart.Items.Count >= CartConsts.MaxItemsInCart)
            throw new BusinessException(string.Format(localizationHelper[CartConsts.ErrorMessages.MaxItemsExceeded], CartConsts.MaxItemsInCart));
    }

    public void CheckMaxQuantityPerItem(int quantity)
    {
        if (quantity > CartConsts.MaxQuantityPerItem)
            throw new BusinessException(string.Format(localizationHelper[CartConsts.ErrorMessages.MaxQuantityExceeded], CartConsts.MaxQuantityPerItem));
    }

    public void CheckMaxTotalAmount(decimal totalAmount)
    {
        if (totalAmount > CartConsts.MaxTotalAmount)
            throw new BusinessException(string.Format(localizationHelper[CartConsts.ErrorMessages.MaxTotalAmountExceeded], CartConsts.MaxTotalAmount));
    }
} 