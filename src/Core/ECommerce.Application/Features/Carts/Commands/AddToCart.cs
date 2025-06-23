using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Features.Carts.DTOs;

using ECommerce.Application.Repositories;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.Carts.Commands;

public sealed record AddToCartCommand(
    Guid ProductId,
    int Quantity) : IRequest<Result<CartSummaryDto>>, IValidatableRequest, ITransactionalRequest;

public sealed class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage(CartConsts.ValidationMessages.ProductIdRequired);

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage(CartConsts.ValidationMessages.QuantityMustBePositive)
            .LessThanOrEqualTo(CartConsts.MaxQuantityPerItem)
            .WithMessage(string.Format(CartConsts.ErrorMessages.MaxQuantityExceeded, CartConsts.MaxQuantityPerItem));
    }
}

public sealed class AddToCartCommandHandler(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    ICurrentUserService currentUserService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<AddToCartCommand, Result<CartSummaryDto>>(lazyServiceProvider), IRequestHandler<AddToCartCommand, Result<CartSummaryDto>>
{
    public override async Task<Result<CartSummaryDto>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var userIdString = currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var currentUserId))
            return Result<CartSummaryDto>.Unauthorized();

        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken: cancellationToken);
        if (product is null)
            return Result<CartSummaryDto>.NotFound(CartConsts.ErrorMessages.ProductNotFound);

        if (!product.IsActive)
            return Result<CartSummaryDto>.Error(CartConsts.ErrorMessages.ProductNotActive);

        if (!product.IsOrderable(request.Quantity))
            return Result<CartSummaryDto>.Error(CartConsts.ErrorMessages.InsufficientStock);

        var cart = await cartRepository.GetByUserIdWithItemsAsync(currentUserId, cancellationToken);
        
        if (cart is null)
        {
            cart = Cart.Create(currentUserId);
            await cartRepository.AddAsync(cart, cancellationToken);
        }

        if (cart.Items.Count >= CartConsts.MaxItemsInCart && !cart.HasItem(request.ProductId))
            return Result<CartSummaryDto>.Error(string.Format(CartConsts.ErrorMessages.MaxItemsExceeded, CartConsts.MaxItemsInCart));

        var existingItem = cart.GetItem(request.ProductId);
        var newQuantity = (existingItem?.Quantity ?? 0) + request.Quantity;
        
        if (newQuantity > CartConsts.MaxQuantityPerItem)
            return Result<CartSummaryDto>.Error(string.Format(CartConsts.ErrorMessages.MaxQuantityExceeded, CartConsts.MaxQuantityPerItem));

        cart.AddItem(request.ProductId, product.Price.Value, request.Quantity);

        if (cart.TotalAmount > CartConsts.MaxTotalAmount)
            return Result<CartSummaryDto>.Error(string.Format(CartConsts.ErrorMessages.MaxTotalAmountExceeded, CartConsts.MaxTotalAmount));

        var summary = new CartSummaryDto(cart.Id, cart.TotalItems, cart.TotalAmount);
        return Result<CartSummaryDto>.Success(summary);
    }
} 