using Ardalis.Result;
using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Features.Products.V1.Commands;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Repositories;
using ECommerce.Domain.Enums;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.Products.V1.Queries;

public sealed record GetProductImagesQuery(
    Guid ProductId,
    ImageType? ImageType = null,
    bool ActiveOnly = true
) : IRequest<Result<List<ProductImageResponse>>>;

public sealed class GetProductImagesQueryValidator : AbstractValidator<GetProductImagesQuery>
{
    public GetProductImagesQueryValidator(
        IProductRepository productRepository,
        ILocalizationHelper localizer)
    {
        RuleFor(x => x.ProductId)
            .MustAsync(async (id, ct) => await productRepository.AnyAsync(x => x.Id == id, cancellationToken: ct))
            .WithMessage(localizer[ProductConsts.NotFound]);
    }
}

public sealed class GetProductImagesQueryHandler(
    IProductRepository productRepository,
    IProductImageRepository productImageRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetProductImagesQuery, Result<List<ProductImageResponse>>>(lazyServiceProvider)
{
    public override async Task<Result<List<ProductImageResponse>>> Handle(GetProductImagesQuery request, CancellationToken cancellationToken)
    {
        // Check if product exists
        var productExists = await productRepository.AnyAsync(x => x.Id == request.ProductId, cancellationToken: cancellationToken);
        if (!productExists)
        {
            return Result<List<ProductImageResponse>>.NotFound(Localizer[ProductConsts.NotFound]);
        }

        // Get images based on criteria
        List<Domain.Entities.ProductImage> images;

        if (request.ImageType.HasValue)
        {
            images = await productImageRepository.GetByImageTypeAsync(
                request.ProductId, 
                request.ImageType.Value, 
                cancellationToken);
        }
        else if (request.ActiveOnly)
        {
            images = await productImageRepository.GetActiveByProductIdAsync(
                request.ProductId, 
                cancellationToken);
        }
        else
        {
            images = await productImageRepository.GetByProductIdAsync(
                request.ProductId, 
                cancellationToken);
        }

        // Map to response DTOs
        var responses = images.Select(image => new ProductImageResponse(
            image.Id,
            image.CloudinaryPublicId,
            image.ImageUrl,
            image.ThumbnailUrl,
            image.LargeUrl,
            image.ImageType,
            image.DisplayOrder,
            image.AltText
        )).ToList();

        return Result<List<ProductImageResponse>>.Success(responses);
    }
} 