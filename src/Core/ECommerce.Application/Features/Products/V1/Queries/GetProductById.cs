using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.Common.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Features.Products.Specifications;
using ECommerce.Application.Features.Products.V1.DTOs;
using ECommerce.Application.Repositories;
using ECommerce.SharedKernel;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Products.V1.Queries;

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
        var spec = new ProductByIdSpecification(query.Id);
        var products = await productRepository.ListAsync(spec, cancellationToken);
        var product = products.FirstOrDefault();

        if (product is null)
            return Result.NotFound(Localizer[ProductConsts.NotFound]);

        return Result.Success(product.Adapt<ProductDto>());
    }
}