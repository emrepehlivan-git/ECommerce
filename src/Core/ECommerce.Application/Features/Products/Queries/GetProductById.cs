using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Features.Products.DTOs;
using ECommerce.Application.Repositories;
using ECommerce.SharedKernel;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Products.Queries;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDto>>, ICacheableRequest
{
    public string CacheKey => $"product:{Id}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(15);
}

public sealed class GetProductByIdQueryHandler(
    IProductRepository productRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetProductByIdQuery, Result<ProductDto>>(lazyServiceProvider)
{
    public override async Task<Result<ProductDto>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(query.Id,
            include: x => x.Include(p => p.Category)
            .Include(p => p.Stock),
            cancellationToken: cancellationToken);

        if (product is null)
            return Result.NotFound(Localizer[ProductConsts.NotFound]);

        return Result.Success(product.Adapt<ProductDto>());
    }
}