using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Features.Carts.DTOs;
using ECommerce.Application.Helpers;
using ECommerce.Application.Repositories;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Carts.Commands;

public sealed record UpdateCartItemQuantityCommand(
    Guid ProductId,
    int Quantity) : IRequest<Result<CartSummaryDto>>, IValidatableRequest, ITransactionalRequest;

public sealed class UpdateCartItemQuantityCommandValidator : AbstractValidator<UpdateCartItemQuantityCommand>
{
    public UpdateCartItemQuantityCommandValidator(LocalizationHelper localizer)
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage(localizer[CartConsts.ValidationMessages.ProductIdRequired]);

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage(localizer[CartConsts.ValidationMessages.QuantityMustBePositive])
            .LessThanOrEqualTo(CartConsts.MaxQuantityPerItem)
            .WithMessage(string.Format(localizer[CartConsts.ErrorMessages.MaxQuantityExceeded], CartConsts.MaxQuantityPerItem));
    }
}

public sealed class UpdateCartItemQuantityCommandHandler(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    ICurrentUserService currentUserService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<UpdateCartItemQuantityCommand, Result<CartSummaryDto>>(lazyServiceProvider)
{
    public override async Task<Result<CartSummaryDto>> Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        var userIdString = currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var currentUserId))
            return Result<CartSummaryDto>.Unauthorized();

        var cart = await cartRepository.GetByUserIdWithItemsAsync(currentUserId, cancellationToken);
        if (cart is null)
            return Result<CartSummaryDto>.NotFound(Localizer[CartConsts.ErrorMessages.CartNotFound]);

        if (!cart.HasItem(request.ProductId))
            return Result<CartSummaryDto>.NotFound(Localizer[CartConsts.ErrorMessages.CartItemNotFound]);

        var product = await productRepository.GetByIdAsync(request.ProductId, 
            include: x => x.Include(p => p.Stock), 
            cancellationToken: cancellationToken);
        if (product is null)
            return Result<CartSummaryDto>.NotFound(Localizer[CartConsts.ErrorMessages.ProductNotFound]);

        if (!product.IsOrderable(request.Quantity))
            return Result<CartSummaryDto>.Error(Localizer[CartConsts.ErrorMessages.InsufficientStock]);

        cart.UpdateItemQuantity(request.ProductId, request.Quantity);

        if (cart.TotalAmount > CartConsts.MaxTotalAmount)
            return Result<CartSummaryDto>.Error(string.Format(Localizer[CartConsts.ErrorMessages.MaxTotalAmountExceeded], CartConsts.MaxTotalAmount));

        var summary = new CartSummaryDto(cart.Id, cart.TotalItems, cart.TotalAmount);
        return Result<CartSummaryDto>.Success(summary);
    }
} 