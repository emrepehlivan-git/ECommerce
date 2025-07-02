using ECommerce.Application.Repositories;
using ECommerce.SharedKernel;
using MediatR;
using Ardalis.Result;
using ECommerce.Application.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ECommerce.Application.Features.Products;

namespace ECommerce.Application.Features.Stock.V1.Queries;

public sealed record GetProductStockInfo(Guid ProductId) : IRequest<Result<int>>;
public sealed class GetProductStockInfoHandler(
    IProductRepository productRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetProductStockInfo, Result<int>>(lazyServiceProvider)
{
    public override async Task<Result<int>> Handle(GetProductStockInfo request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(
            request.ProductId,
            include: x => x.Include(p => p.Stock),
            cancellationToken: cancellationToken);

        return product is null
            ? Result.NotFound(Localizer[ProductConsts.NotFound])
            : Result.Success(product.Stock.Quantity);
    }
}
