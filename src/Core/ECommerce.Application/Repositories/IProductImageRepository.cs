using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.SharedKernel.Repositories;

namespace ECommerce.Application.Repositories;

public interface IProductImageRepository : IRepository<ProductImage>
{
    Task<List<ProductImage>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<List<ProductImage>> GetActiveByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<ProductImage?> GetByCloudinaryPublicIdAsync(string publicId, CancellationToken cancellationToken = default);
    Task<ProductImage?> GetMainImageByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<List<ProductImage>> GetByImageTypeAsync(Guid productId, ImageType imageType, CancellationToken cancellationToken = default);
    Task<int> GetNextDisplayOrderAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCloudinaryPublicIdAsync(string publicId, CancellationToken cancellationToken = default);
    Task DeleteByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
} 