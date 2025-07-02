using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Repositories;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using MediatR;

namespace ECommerce.Application.Features.Carts.V1.Commands;

public sealed record ClearCartCommand() : IRequest<Result>, ITransactionalRequest;

public sealed class ClearCartCommandHandler(
    ICartRepository cartRepository,
    ICurrentUserService currentUserService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<ClearCartCommand, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var userIdString = currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var currentUserId))
            return Result.Unauthorized();

        var cart = await cartRepository.GetByUserIdWithItemsAsync(currentUserId, cancellationToken);
        if (cart is null)
            return Result.NotFound(Localizer[CartConsts.ErrorMessages.CartNotFound]);

        cart.Clear();

        return Result.Success();
    }
} 