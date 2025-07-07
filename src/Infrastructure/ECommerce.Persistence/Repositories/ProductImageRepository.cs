using ECommerce.Application.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Persistence.Contexts;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Persistence.Repositories;

public sealed class ProductImageRepository(ApplicationDbContext context) 
    : BaseRepository<ProductImage>(context), IProductImageRepository, IScopedDependency
{
    public async Task<List<ProductImage>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await Context.ProductImages
            .Where(pi => pi.ProductId == productId)
            .OrderBy(pi => pi.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ProductImage>> GetActiveByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await Context.ProductImages
            .Where(pi => pi.ProductId == productId && pi.IsActive)
            .OrderBy(pi => pi.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductImage?> GetByCloudinaryPublicIdAsync(string publicId, CancellationToken cancellationToken = default)
    {
        return await Context.ProductImages
            .FirstOrDefaultAsync(pi => pi.CloudinaryPublicId == publicId, cancellationToken);
    }

    public async Task<ProductImage?> GetMainImageByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await Context.ProductImages
            .Where(pi => pi.ProductId == productId && pi.IsActive && pi.ImageType == ImageType.Main)
            .OrderBy(pi => pi.DisplayOrder)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<ProductImage>> GetByImageTypeAsync(Guid productId, ImageType imageType, CancellationToken cancellationToken = default)
    {
        return await Context.ProductImages
            .Where(pi => pi.ProductId == productId && pi.IsActive && pi.ImageType == imageType)
            .OrderBy(pi => pi.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetNextDisplayOrderAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var maxOrder = await Context.ProductImages
            .Where(pi => pi.ProductId == productId)
            .MaxAsync(pi => (int?)pi.DisplayOrder, cancellationToken);

        return (maxOrder ?? -1) + 1;
    }

    public async Task<bool> ExistsByCloudinaryPublicIdAsync(string publicId, CancellationToken cancellationToken = default)
    {
        return await Context.ProductImages
            .AnyAsync(pi => pi.CloudinaryPublicId == publicId, cancellationToken);
    }

    public async Task DeleteByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var images = await Context.ProductImages
            .Where(pi => pi.ProductId == productId)
            .ToListAsync(cancellationToken);

        Context.ProductImages.RemoveRange(images);
    }
} 