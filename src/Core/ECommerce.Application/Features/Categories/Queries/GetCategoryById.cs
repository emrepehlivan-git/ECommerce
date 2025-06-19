using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Features.Categories.DTOs;
using ECommerce.Application.Repositories;
using ECommerce.SharedKernel.DependencyInjection;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace ECommerce.Application.Features.Categories.Queries;

public sealed record GetCategoryByIdQuery(Guid Id) : IRequest<Result<CategoryDto>>, ICacheableRequest
{
    public string CacheKey => $"category:{Id}";
    public TimeSpan CacheDuration => TimeSpan.FromHours(2);
}

public sealed class GetCategoryByIdQueryHandler(
    ICategoryRepository categoryRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetCategoryByIdQuery, Result<CategoryDto>>(lazyServiceProvider)
{
    public override async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery query, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(query.Id, cancellationToken: cancellationToken);

        if (category is null)
            return Result.NotFound(Localizer[CategoryConsts.NotFound]);


        return Result.Success(category.Adapt<CategoryDto>());
    }
}