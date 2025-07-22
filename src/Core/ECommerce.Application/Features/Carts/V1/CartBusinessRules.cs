using ECommerce.Application.Exceptions;
using ECommerce.Application.Features.Carts.V1.Specifications;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Repositories;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Features.Carts.V1;

public sealed class CartBusinessRules(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    ILocalizationHelper ILocalizationHelper)
{
    public async Task CheckCartExistsAsync(Guid cartId)
    {
        _ = await cartRepository.GetByIdAsync(cartId) ?? throw new NotFoundException(ILocalizationHelper[CartConsts.ErrorMessages.CartNotFound]);
    }

    public async Task CheckProductCanBeOrderedAsync(Guid productId, int quantity)
    {
        var spec = new CartItemOrderableSpecification(productId, quantity);
        var canOrder = await productRepository.AnyAsync(spec);
        
        if (!canOrder)
        {
            var productExists = await productRepository.GetByIdAsync(productId);
            if (productExists is null)
                throw new NotFoundException(ILocalizationHelper[CartConsts.ErrorMessages.ProductNotFound]);
            if (!productExists.IsActive)
                throw new BusinessException(ILocalizationHelper[CartConsts.ErrorMessages.ProductNotActive]);
            throw new BusinessException(ILocalizationHelper[CartConsts.ErrorMessages.InsufficientStock]);
        }
    }

    public void CheckMaxItemsInCart(Cart cart, bool isNewItem)
    {
        if (isNewItem && cart.Items.Count >= CartConsts.MaxItemsInCart)
            throw new BusinessException(string.Format(ILocalizationHelper[CartConsts.ErrorMessages.MaxItemsExceeded], CartConsts.MaxItemsInCart));
    }

    public void CheckMaxQuantityPerItem(int quantity)
    {
        if (quantity > CartConsts.MaxQuantityPerItem)
            throw new BusinessException(string.Format(ILocalizationHelper[CartConsts.ErrorMessages.MaxQuantityExceeded], CartConsts.MaxQuantityPerItem));
    }

    public void CheckMaxTotalAmount(decimal totalAmount)
    {
        if (totalAmount > CartConsts.MaxTotalAmount)
            throw new BusinessException(string.Format(ILocalizationHelper[CartConsts.ErrorMessages.MaxTotalAmountExceeded], CartConsts.MaxTotalAmount));
    }
} 