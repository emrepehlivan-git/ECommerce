using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Features.Carts.V1.DTOs;
using ECommerce.Application.Repositories;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using Mapster;
using MediatR;

namespace ECommerce.Application.Features.Carts.V1.Queries;

public sealed record GetCartQuery() : IRequest<Result<CartDto>>, ICacheableRequest
{
    public string CacheKey => "cart:current_user";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(2);
}

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
            return Result<CartDto>.Success(new CartDto());
        }

        var cartDto = cart.Adapt<CartDto>();
        return Result<CartDto>.Success(cartDto);
    }
} 