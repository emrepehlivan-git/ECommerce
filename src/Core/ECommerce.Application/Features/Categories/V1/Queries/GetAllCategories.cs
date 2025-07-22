using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Extensions;
using ECommerce.Application.Features.Categories.Specifications;
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
        var spec = new CategorySearchSpecification(query.PageableRequestParams.Search);
        
        return await categoryRepository.GetPagedAsync<CategoryDto>(
            specification: spec,
            page: query.PageableRequestParams.Page,
            pageSize: query.PageableRequestParams.PageSize,
            cancellationToken: cancellationToken);
    }
}