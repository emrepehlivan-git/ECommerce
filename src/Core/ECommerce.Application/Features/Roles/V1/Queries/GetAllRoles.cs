using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Features.Roles.V1.DTOs;
using ECommerce.Application.Parameters;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using Mapster;
using MediatR;

namespace ECommerce.Application.Features.Roles.V1.Queries;

public sealed record GetAllRolesQuery (PageableRequestParams pageableRequestParams, bool IncludePermissions = false): IRequest<PagedResult<List<RoleDto>>>, ICacheableRequest
{
    public string CacheKey => $"roles:all:include-permissions:{IncludePermissions}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(30);
}

public sealed class GetAllRolesQueryHandler(
    IRoleService roleService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetAllRolesQuery, PagedResult<List<RoleDto>>>(lazyServiceProvider)
{
    public override async Task<PagedResult<List<RoleDto>>> Handle(GetAllRolesQuery query, CancellationToken cancellationToken)
    {
        return await roleService.GetAllRolesAsync(query.pageableRequestParams.Page, query.pageableRequestParams.PageSize, query.pageableRequestParams?.Search ?? string.Empty, query.IncludePermissions);
    }
} 