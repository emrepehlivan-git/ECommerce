using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.Common.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Features.Products;
using ECommerce.Application.Helpers;
using ECommerce.Application.Repositories;
using ECommerce.SharedKernel;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Stock.V1.Commands;

public record class UpdateProductStock(Guid ProductId, int StockQuantity) : IRequest<Result>, IValidatableRequest, ITransactionalRequest
;

public sealed class UpdateProductStockValidator : AbstractValidator<UpdateProductStock>
{
    public UpdateProductStockValidator(IProductRepository productRepository, LocalizationHelper localizer)
    {
        RuleFor(x => x.ProductId)
            .MustAsync(async (id, ct) => await productRepository.AnyAsync(x => x.Id == id, cancellationToken: ct))
            .WithMessage(localizer[ProductConsts.NotFound]);

        RuleFor(x => x.StockQuantity)
            .GreaterThan(0)
            .WithMessage(localizer[ProductConsts.StockQuantityMustBeGreaterThanZero]);
    }
}

public sealed class UpdateProductStockHandler(
    IProductRepository productRepository,
    IStockRepository stockRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<UpdateProductStock, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(UpdateProductStock request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, 
            include: x => x.Include(p => p.Stock), 
            cancellationToken: cancellationToken);

        if (product is null)
            return Result.NotFound(Localizer[ProductConsts.NotFound]);

        await stockRepository.ReserveStockAsync(request.ProductId, request.StockQuantity, cancellationToken);

        return Result.Success();
    }
}