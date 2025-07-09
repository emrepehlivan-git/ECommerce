using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Features.Categories.V1.DTOs;
using ECommerce.Application.Repositories;
using ECommerce.SharedKernel.DependencyInjection;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace ECommerce.Application.Features.Categories.V1.Queries;

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