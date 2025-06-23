using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Features.Carts.DTOs;

using ECommerce.Application.Repositories;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.Carts.Commands;

public sealed record RemoveFromCartCommand(
    Guid ProductId) : IRequest<Result<CartSummaryDto>>, IValidatableRequest, ITransactionalRequest;

public sealed class RemoveFromCartCommandValidator : AbstractValidator<RemoveFromCartCommand>
{
    public RemoveFromCartCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage(CartConsts.ValidationMessages.ProductIdRequired);
    }
}

public sealed class RemoveFromCartCommandHandler(
    ICartRepository cartRepository,
    ICurrentUserService currentUserService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<RemoveFromCartCommand, Result<CartSummaryDto>>(lazyServiceProvider)
{
    public override async Task<Result<CartSummaryDto>> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        var userIdString = currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var currentUserId))
            return Result<CartSummaryDto>.Unauthorized();

        var cart = await cartRepository.GetByUserIdWithItemsAsync(currentUserId, cancellationToken);
        if (cart is null)
            return Result<CartSummaryDto>.NotFound(CartConsts.ErrorMessages.CartNotFound);

        if (!cart.HasItem(request.ProductId))
            return Result<CartSummaryDto>.NotFound(CartConsts.ErrorMessages.CartItemNotFound);

        cart.RemoveItem(request.ProductId);

        var summary = new CartSummaryDto(cart.Id, cart.TotalItems, cart.TotalAmount);
        return Result<CartSummaryDto>.Success(summary);
    }
} 