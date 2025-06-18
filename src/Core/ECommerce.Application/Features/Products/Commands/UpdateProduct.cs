using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Helpers;
using ECommerce.Application.Repositories;
using ECommerce.SharedKernel;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.Products.Commands;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    Guid CategoryId) : IRequest<Result>, IValidatableRequest, ITransactionalRequest;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        LocalizationHelper localizer)
    {
        RuleFor(x => x.Id)
            .MustAsync(async (id, ct) =>
                await productRepository.AnyAsync(x => x.Id == id, cancellationToken: ct))
            .WithMessage(localizer[ProductConsts.NotFound]);

        RuleFor(x => x.Name)
            .MinimumLength(ProductConsts.NameMinLength)
            .WithMessage(localizer[ProductConsts.NameMustBeAtLeastCharacters])
            .MaximumLength(ProductConsts.NameMaxLength)
            .WithMessage(localizer[ProductConsts.NameMustBeLessThanCharacters])
            .MustAsync(async (command, name, ct) =>
                !await productRepository.AnyAsync(x => x.Name.ToLower() == name.ToLower() && x.Id != command.Id, cancellationToken: ct))
            .WithMessage(localizer[ProductConsts.NameExists]);

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage(localizer[ProductConsts.PriceMustBeGreaterThanZero]);

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .MustAsync(async (id, ct) =>
                await categoryRepository.AnyAsync(x => x.Id == id, cancellationToken: ct))
            .WithMessage(localizer[ProductConsts.CategoryNotFound]);
    }
}

public sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<UpdateProductCommand, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(command.Id, cancellationToken: cancellationToken);

        if (product is null)
            return Result.NotFound(Localizer[ProductConsts.NotFound]);

        product!.Update(command.Name, command.Price, command.CategoryId, command.Description);

        productRepository.Update(product);

        return Result.Success();
    }
}