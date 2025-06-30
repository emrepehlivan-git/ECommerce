using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Extensions;
using ECommerce.Application.Repositories;
using ECommerce.Application.Features.Categories.DTOs;
using ECommerce.SharedKernel.DependencyInjection;
using MediatR;
using ECommerce.Application.Parameters;

namespace ECommerce.Application.Features.Categories.Queries;

public sealed record GetAllCategoriesQuery(PageableRequestParams PageableRequestParams, string? OrderBy = null) : IRequest<PagedResult<List<CategoryDto>>>, ICacheableRequest
{
    public string CacheKey => $"categories:page-{PageableRequestParams.Page}:size-{PageableRequestParams.PageSize}:order-{OrderBy ?? "default"}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(30);
}

public sealed class GetAllCategoriesQueryHandler(
    ICategoryRepository categoryRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetAllCategoriesQuery, PagedResult<List<CategoryDto>>>(lazyServiceProvider)
{
    public override async Task<PagedResult<List<CategoryDto>>> Handle(GetAllCategoriesQuery query, CancellationToken cancellationToken)
    {
        return await categoryRepository.GetPagedAsync<CategoryDto>(
            orderBy: x => x.ApplyOrderBy(Filter.FromOrderByString(query.OrderBy)),
            predicate: x => string.IsNullOrEmpty(query.PageableRequestParams.Search) ? true : x.Name.ToLower().Contains(query.PageableRequestParams.Search.ToLower()),
            page: query.PageableRequestParams.Page,
            pageSize: query.PageableRequestParams.PageSize,
            cancellationToken: cancellationToken);
    }
}