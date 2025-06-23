using Ardalis.Result;
using ECommerce.Application.CQRS;
using ECommerce.Application.Features.Carts.DTOs;
using ECommerce.Application.Repositories;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using Mapster;
using MediatR;

namespace ECommerce.Application.Features.Carts.Queries;

public sealed record GetCartQuery() : IRequest<Result<CartDto>>;

public sealed class GetCartQueryHandler(
    ICartRepository cartRepository,
    ICurrentUserService currentUserService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetCartQuery, Result<CartDto>>(lazyServiceProvider)
{
    public override async Task<Result<CartDto>> Handle(GetCartQuery query, CancellationToken cancellationToken)
    {
        var userIdString = currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var currentUserId))
            return Result<CartDto>.Unauthorized();

        var cart = await cartRepository.GetByUserIdWithItemsAsync(currentUserId, cancellationToken);
        
        if (cart is null)
        {
            return Result<CartDto>.NotFound(CartConsts.ErrorMessages.CartNotFound);
        }

        var cartDto = cart.Adapt<CartDto>();
        return Result<CartDto>.Success(cartDto);
    }
} 