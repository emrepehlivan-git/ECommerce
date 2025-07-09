using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.Common.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Features.Products;
using ECommerce.Application.Interfaces;
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
    public UpdateProductStockValidator(ILocalizationHelper localizer)
    {
        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage(localizer[ProductConsts.StockQuantityMustBeGreaterThanZero]);
    }
}

public sealed class UpdateProductStockHandler(
    IProductRepository productRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<UpdateProductStock, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(UpdateProductStock request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, 
            include: x => x.Include(p => p.Stock),
            isTracking: true,
            cancellationToken: cancellationToken);

        if (product is null || product.Stock is null)
            return Result.NotFound(Localizer[ProductConsts.NotFound]);

        product.Stock.UpdateQuantity(request.StockQuantity);

        return Result.Success();
    }
}