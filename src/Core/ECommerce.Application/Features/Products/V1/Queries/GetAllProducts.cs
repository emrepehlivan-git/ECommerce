using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Extensions;
using ECommerce.Application.Repositories;
using ECommerce.Application.Features.Products.V1.DTOs;
using ECommerce.Application.Parameters;
using MediatR;
using System.Linq.Expressions;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Features.Products.V1.Queries;

public sealed record GetAllProductsQuery(
    PageableRequestParams PageableRequestParams, 
    string? OrderBy = null,
    Guid? CategoryId = null
) : IRequest<PagedResult<List<ProductDto>>>, ICacheableRequest
{
    public string CacheKey => $"products:page-{PageableRequestParams.Page}:size-{PageableRequestParams.PageSize}:search-{PageableRequestParams.Search ?? "empty"}:order-{OrderBy ?? "default"}:categoryId-{CategoryId?.ToString() ?? "all"}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}

public sealed class GetAllProductsQueryHandler(
    IProductRepository productRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetAllProductsQuery, PagedResult<List<ProductDto>>>(lazyServiceProvider)
{
    public override async Task<PagedResult<List<ProductDto>>> Handle(GetAllProductsQuery query, CancellationToken cancellationToken)
    {
        Expression<Func<Product, bool>>? predicate = null;

        if (query.CategoryId.HasValue && !string.IsNullOrWhiteSpace(query.PageableRequestParams.Search))
        {
            predicate = p => p.CategoryId == query.CategoryId.Value && 
                           p.Name.ToLower().Contains(query.PageableRequestParams.Search.ToLower());
        }
        else if (query.CategoryId.HasValue)
        {
            predicate = p => p.CategoryId == query.CategoryId.Value;
        }
        else if (!string.IsNullOrWhiteSpace(query.PageableRequestParams.Search))
        {
            predicate = p => p.Name.ToLower().Contains(query.PageableRequestParams.Search.ToLower());
        }

        return await productRepository.GetPagedAsync<ProductDto>(
            predicate: predicate,
            orderBy: x => x.ApplyOrderBy(Filter.FromOrderByString(query.OrderBy)),
            page: query.PageableRequestParams.Page,
            pageSize: query.PageableRequestParams.PageSize,
            cancellationToken: cancellationToken);
    }
}