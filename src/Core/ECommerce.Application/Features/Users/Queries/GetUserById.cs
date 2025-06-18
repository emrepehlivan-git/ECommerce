using Ardalis.Result;
using ECommerce.Application.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Features.Users.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.SharedKernel;
using Mapster;
using MediatR;

namespace ECommerce.Application.Features.Users.Queries;


public sealed record GetUserByIdQuery(Guid UserId) : IRequest<Result<UserDto>>;

public sealed class GetUserByIdQueryHandler(
    IIdentityService identityService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetUserByIdQuery, Result<UserDto>>(lazyServiceProvider)
{
    public override async Task<Result<UserDto>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByIdAsync(query.UserId);

        if (user is null)
            return Result.NotFound(Localizer[UserConsts.NotFound]);

        return Result.Success(user.Adapt<UserDto>());
    }
}
