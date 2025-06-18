using Ardalis.Result;
using ECommerce.Application.CQRS;
using ECommerce.Application.Extensions;
using ECommerce.Application.Repositories;
using ECommerce.Application.Features.Categories.DTOs;
using ECommerce.SharedKernel.DependencyInjection;
using MediatR;
using ECommerce.Application.Parameters;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Features.Categories.Queries;

public sealed record GetAllCategoriesQuery(PageableRequestParams PageableRequestParams, string? OrderBy = null) : IRequest<PagedResult<List<CategoryDto>>>;

public sealed class GetAllCategoriesQueryHandler(
    ICategoryRepository categoryRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetAllCategoriesQuery, PagedResult<List<CategoryDto>>>(lazyServiceProvider)
{
    public override async Task<PagedResult<List<CategoryDto>>> Handle(GetAllCategoriesQuery query, CancellationToken cancellationToken)
    {
        return await categoryRepository.Query(
            orderBy: x => x.ApplyOrderBy(Filter.FromOrderByString(query.OrderBy)))
            .ApplyPagingAsync<Category, CategoryDto>(query.PageableRequestParams, cancellationToken: cancellationToken);
    }
}