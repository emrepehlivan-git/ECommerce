using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Helpers;
using ECommerce.Application.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.SharedKernel;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.Products.Commands;

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    Guid CategoryId,
    int StockQuantity) : IRequest<Result<Guid>>, IValidatableRequest, ITransactionalRequest;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator(
        ICategoryRepository categoryRepository,
        LocalizationHelper localizer)
    {
        RuleFor(x => x.Name)
            .MinimumLength(ProductConsts.NameMinLength)
            .WithMessage(string.Format(localizer[ProductConsts.NameMustBeAtLeastCharacters], ProductConsts.NameMinLength))
            .MaximumLength(ProductConsts.NameMaxLength)
            .WithMessage(string.Format(localizer[ProductConsts.NameMustBeLessThanCharacters], ProductConsts.NameMaxLength));

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage(localizer[ProductConsts.PriceMustBeGreaterThanZero]);

        RuleFor(x => x.CategoryId)
            .MustAsync(async (id, ct) =>
                await categoryRepository.AnyAsync(x => x.Id == id, cancellationToken: ct))
            .WithMessage(localizer[ProductConsts.CategoryNotFound]);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage(localizer[ProductConsts.StockQuantityMustBeGreaterThanZero]);
    }
}

public sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IStockRepository stockRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<CreateProductCommand, Result<Guid>>(lazyServiceProvider)
{
    public override Task<Result<Guid>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = Product.Create(
            command.Name,
            command.Description,
            command.Price,
            command.CategoryId,
            command.StockQuantity);

        productRepository.Add(product);
        stockRepository.Add(ProductStock.Create(product.Id, command.StockQuantity));

        return Task.FromResult(Result.Success(product.Id));
    }
}