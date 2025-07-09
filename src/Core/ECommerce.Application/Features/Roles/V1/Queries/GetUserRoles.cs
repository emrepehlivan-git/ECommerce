using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Features.Roles.V1.DTOs;
using ECommerce.Application.Services;
using ECommerce.Application.Helpers;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.Roles.V1.Queries;

public sealed record GetUserRolesQuery(Guid UserId) : IRequest<Result<UserRoleDto>>, ICacheableRequest
{
    public string CacheKey => $"user-roles:{UserId}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(15);
}

public sealed class GetUserRolesQueryValidator : AbstractValidator<GetUserRolesQuery>
{
    public GetUserRolesQueryValidator(LocalizationHelper localizer)
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(localizer[RoleConsts.UserNotFound]);
    }
}

public sealed class GetUserRolesQueryHandler(
    IRoleService roleService,
    IUserService userService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetUserRolesQuery, Result<UserRoleDto>>(lazyServiceProvider)
{
    public override async Task<Result<UserRoleDto>> Handle(GetUserRolesQuery query, CancellationToken cancellationToken)
    {
        var user = await userService.FindByIdAsync(query.UserId);
        if (user == null)
        {
            return Result.Error(Localizer[RoleConsts.UserNotFound]);
        }

        var userRoles = await roleService.GetUserRolesAsync(user);
        
        var userRoleDto = new UserRoleDto(
            user.Id,
            user.UserName!,
            userRoles.ToList());
        
        return Result.Success(userRoleDto);
    }
} 