using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Repositories;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.Products.V1.Commands;

public sealed record DeleteProductImageCommand(
    Guid ProductId,
    Guid ImageId
) : IRequest<Result>, IValidatableRequest, ITransactionalRequest;

public sealed class DeleteProductImageCommandValidator : AbstractValidator<DeleteProductImageCommand>
{
    public DeleteProductImageCommandValidator(
        IProductRepository productRepository,
        IProductImageRepository productImageRepository,
        ILocalizationHelper localizer)
    {
        RuleFor(x => x.ProductId)
            .MustAsync(async (id, ct) => await productRepository.AnyAsync(x => x.Id == id, cancellationToken: ct))
            .WithMessage(localizer[ProductConsts.NotFound]);

        RuleFor(x => x.ImageId)
            .MustAsync(async (command, imageId, ct) =>
            {
                var image = await productImageRepository.GetByIdAsync(imageId, cancellationToken: ct);
                return image != null && image.ProductId == command.ProductId;
            })
            .WithMessage(localizer[ProductConsts.ImageNotFound]);
    }
}

public sealed class DeleteProductImageCommandHandler(
    IProductImageRepository productImageRepository,
    ICloudinaryService cloudinaryService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<DeleteProductImageCommand, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
    {
        // Get the image to delete
        var productImage = await productImageRepository.GetByIdAsync(request.ImageId, cancellationToken: cancellationToken);
        if (productImage == null || productImage.ProductId != request.ProductId)
        {
            return Result.NotFound(Localizer[ProductConsts.ImageNotFound]);
        }

        try
        {
            // Delete from Cloudinary first
            var cloudinaryDeleteResult = await cloudinaryService.DeleteImageAsync(
                productImage.CloudinaryPublicId, 
                cancellationToken);

            if (!cloudinaryDeleteResult)
            {
                return Result.Error(Localizer[ProductConsts.ImageDeleteFailed]);
            }

            // Delete from database
            productImageRepository.Delete(productImage);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Error($"{Localizer[ProductConsts.ImageDeleteFailed]}: {ex.Message}");
        }
    }
} 