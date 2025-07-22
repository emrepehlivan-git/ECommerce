using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.Common.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Extensions;
using ECommerce.Application.Features.Products.Specifications;
using ECommerce.Application.Repositories;
using ECommerce.Application.Features.Products.V1.DTOs;
using ECommerce.Application.Parameters;
using MediatR;
using System.Linq.Expressions;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
        var spec = new ProductFilterSpecification(query.CategoryId, query.PageableRequestParams.Search);

        return await productRepository.GetPagedAsync<ProductDto>(
            specification: spec,
            page: query.PageableRequestParams.Page,
            pageSize: query.PageableRequestParams.PageSize,
            cancellationToken: cancellationToken);
    }
}