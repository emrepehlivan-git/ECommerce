using Ardalis.Result;
using ECommerce.Application.CQRS;
using ECommerce.Application.Features.Users.V1.DTOs;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using Mapster;
using MediatR;

namespace ECommerce.Application.Features.Users.V1.Queries;


public sealed record GetUserByIdQuery(Guid UserId) : IRequest<Result<UserDto>>;

public sealed class GetUserByIdQueryHandler(
    IUserService userService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetUserByIdQuery, Result<UserDto>>(lazyServiceProvider)
{
    public override async Task<Result<UserDto>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await userService.FindByIdAsync(query.UserId);

        if (user is null)
            return Result.NotFound(Localizer[UserConsts.NotFound]);

        return Result.Success(user.Adapt<UserDto>());
    }
}
