using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Repositories;
using ECommerce.Application.Services;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.Products.V1.Commands;

public sealed record UploadProductImagesCommand(
    Guid ProductId,
    List<ProductImageUploadRequest> Images
) : IRequest<Result<List<ProductImageResponse>>>, IValidatableRequest, ITransactionalRequest;

public sealed record ProductImageUploadRequest(
    Stream ImageStream,
    string FileName,
    ImageType ImageType,
    int DisplayOrder,
    string? AltText = null
);

public sealed record ProductImageResponse(
    Guid Id,
    string CloudinaryPublicId,
    string ImageUrl,
    string? ThumbnailUrl,
    string? LargeUrl,
    ImageType ImageType,
    int DisplayOrder,
    string? AltText
);

public sealed class UploadProductImagesValidator : AbstractValidator<UploadProductImagesCommand>
{
    public UploadProductImagesValidator(ILocalizationHelper localizer)
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage(localizer[ProductConsts.NotFound]);

        RuleFor(x => x.Images)
            .NotEmpty()
            .WithMessage(localizer[ProductConsts.ImageNotFound])
            .Must(images => images.Count <= ProductConsts.MaxImagesPerProduct)
            .WithMessage(localizer[ProductConsts.MaxImagesExceeded]);

        RuleForEach(x => x.Images).SetValidator(new ProductImageUploadRequestValidator(localizer));
    }
}

public sealed class ProductImageUploadRequestValidator : AbstractValidator<ProductImageUploadRequest>
{
    public ProductImageUploadRequestValidator(ILocalizationHelper localizer)
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage(localizer[ProductConsts.ImageNotFound]);

        RuleFor(x => x.ImageStream)
            .NotNull()
            .WithMessage(localizer[ProductConsts.ImageNotFound]);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage(localizer[ProductConsts.ImageNotFound]);

        RuleFor(x => x.AltText)
            .MaximumLength(ProductConsts.AltTextMaxLength)
            .WithMessage(localizer[ProductConsts.NormalizeMaxLength]);
    }
}

public sealed class UploadProductImagesHandler(
    IProductRepository productRepository,
    IProductImageRepository productImageRepository,
    ICloudinaryService cloudinaryService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<UploadProductImagesCommand, Result<List<ProductImageResponse>>>(lazyServiceProvider)
{
    public override async Task<Result<List<ProductImageResponse>>> Handle(UploadProductImagesCommand request, CancellationToken cancellationToken)

    {
        // Check if product exists
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken: cancellationToken);
        if (product == null)
        {
            return Result<List<ProductImageResponse>>.NotFound(Localizer[ProductConsts.NotFound]);
        }

        // Check current image count
        var currentImageCount = await productImageRepository.GetActiveByProductIdAsync(request.ProductId, cancellationToken);
        if (currentImageCount.Count + request.Images.Count > ProductConsts.MaxImagesPerProduct)
        {
            return Result<List<ProductImageResponse>>.Error(Localizer[ProductConsts.MaxImagesExceeded]);
        }

        var responses = new List<ProductImageResponse>();
        var uploadedImages = new List<ProductImage>();

        try
        {
            foreach (var imageRequest in request.Images)
            {
                // Upload to Cloudinary
                var uploadResult = await cloudinaryService.UploadImageAsync(
                    imageRequest.ImageStream, 
                    imageRequest.FileName, 
                    imageRequest.ImageType,
                    imageRequest.AltText,
                    cancellationToken);

                if (!uploadResult.IsSuccessful)
                {
                    // Cleanup any previously uploaded images
                    await CleanupUploadedImages(uploadedImages, cloudinaryService, cancellationToken);
                    return Result<List<ProductImageResponse>>.Error(uploadResult.ErrorMessage ?? Localizer[ProductConsts.ImageUploadFailed]);
                }

                // Get next display order if not specified
                var displayOrder = imageRequest.DisplayOrder;
                if (displayOrder == 0)
                {
                    displayOrder = await productImageRepository.GetNextDisplayOrderAsync(request.ProductId, cancellationToken);
                }

                // Create ProductImage entity
                var productImage = ProductImage.Create(
                    request.ProductId,
                    uploadResult.PublicId,
                    uploadResult.SecureUrl,
                    uploadResult.ThumbnailUrl,
                    uploadResult.LargeUrl,
                    displayOrder,
                    imageRequest.ImageType,
                    uploadResult.FileSizeBytes,
                    imageRequest.AltText
                );

                uploadedImages.Add(productImage);
                await productImageRepository.AddAsync(productImage, cancellationToken);

                responses.Add(new ProductImageResponse(
                    productImage.Id,
                    productImage.CloudinaryPublicId,
                    productImage.ImageUrl,
                    productImage.ThumbnailUrl,
                    productImage.LargeUrl,
                    productImage.ImageType,
                    productImage.DisplayOrder,
                    productImage.AltText
                ));
            }

            return Result<List<ProductImageResponse>>.Success(responses);
        }
        catch (Exception ex)
        {
            // Cleanup any uploaded images on error
            await CleanupUploadedImages(uploadedImages, cloudinaryService, cancellationToken);
            return Result<List<ProductImageResponse>>.Error($"Upload failed: {ex.Message}");
        }
    }

    private static async Task CleanupUploadedImages(List<ProductImage> uploadedImages, ICloudinaryService cloudinaryService, CancellationToken cancellationToken)
    {
        foreach (var image in uploadedImages)
        {
            try
            {
                await cloudinaryService.DeleteImageAsync(image.CloudinaryPublicId, cancellationToken);
            }
            catch
            {
                // Log error but don't throw
            }
        }
    }
}