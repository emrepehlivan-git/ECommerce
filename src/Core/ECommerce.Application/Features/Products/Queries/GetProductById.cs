using Ardalis.Result;
using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Features.Products.DTOs;
using ECommerce.Application.Repositories;
using ECommerce.SharedKernel;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Products.Queries;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDto>>;

public sealed class GetProductByIdQueryHandler(
    IProductRepository productRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetProductByIdQuery, Result<ProductDto>>(lazyServiceProvider)
{
    public override async Task<Result<ProductDto>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await productRepository.Query()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);

        if (product is null)
            return Result.NotFound(Localizer[ProductConsts.NotFound]);

        return Result.Success(product.Adapt<ProductDto>());
    }
}