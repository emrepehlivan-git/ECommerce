using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Extensions;
using ECommerce.Application.Repositories;
using ECommerce.Application.Features.Categories.V1.DTOs;
using ECommerce.SharedKernel.DependencyInjection;
using MediatR;
using ECommerce.Application.Parameters;
using System.Linq.Expressions;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Features.Categories.V1.Queries;

public sealed record GetAllCategoriesQuery(PageableRequestParams PageableRequestParams, string? OrderBy = null) : IRequest<PagedResult<List<CategoryDto>>>, ICacheableRequest
{
    public string CacheKey => $"categories:page-{PageableRequestParams.Page}:size-{PageableRequestParams.PageSize}:search-{PageableRequestParams.Search ?? "empty"}:order-{OrderBy ?? "default"}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(30);
}

public sealed class GetAllCategoriesQueryHandler(
    ICategoryRepository categoryRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetAllCategoriesQuery, PagedResult<List<CategoryDto>>>(lazyServiceProvider)
{
    public override async Task<PagedResult<List<CategoryDto>>> Handle(GetAllCategoriesQuery query, CancellationToken cancellationToken)
    {
        Expression<Func<Category, bool>>? predicate = null;

        if (!string.IsNullOrWhiteSpace(query.PageableRequestParams.Search))
            predicate = x => x.Name.ToLower().Contains(query.PageableRequestParams.Search.ToLower());

        return await categoryRepository.GetPagedAsync<CategoryDto>(
            orderBy: x => x.ApplyOrderBy(Filter.FromOrderByString(query.OrderBy)),
            predicate: predicate,
            page: query.PageableRequestParams.Page,
            pageSize: query.PageableRequestParams.PageSize,
            cancellationToken: cancellationToken);
    }
}