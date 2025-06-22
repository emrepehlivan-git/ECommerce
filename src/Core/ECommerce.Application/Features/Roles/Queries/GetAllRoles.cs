using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Features.Roles.DTOs;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using Mapster;
using MediatR;

namespace ECommerce.Application.Features.Roles.Queries;

public sealed record GetAllRolesQuery : IRequest<Result<List<RoleDto>>>, ICacheableRequest
{
    public string CacheKey => "roles:all";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(30);
}

public sealed class GetAllRolesQueryHandler(
    IRoleService roleService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetAllRolesQuery, Result<List<RoleDto>>>(lazyServiceProvider)
{
    public override async Task<Result<List<RoleDto>>> Handle(GetAllRolesQuery query, CancellationToken cancellationToken)
    {
        var roles = await roleService.GetAllRolesAsync();
        var roleDtos = roles.Adapt<List<RoleDto>>();

        return Result.Success(roleDtos);
    }
} 