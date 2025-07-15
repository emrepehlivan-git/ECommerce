using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Common.Logging;
using ECommerce.Application.Services;
using ECommerce.Persistence.Contexts;
using ECommerce.Persistence.Seeders;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.WebAPI.Controllers.V1;

[ApiController]
[Route("api/v1/seeder")]
public sealed class SeederController(ApplicationDbContext context, IECommerceLogger<DataSeeder> logger, ICloudinaryService cloudinaryService) : ControllerBase
{
    [HttpPost("run")]
    public async Task<IActionResult> RunSeeder()
    {
        var seeder = new DataSeeder(context, cloudinaryService, logger);
        await seeder.SeedAsync();
        return Ok("Seeder tetiklendi.");
    }

    [HttpPost("seed-images")]
    public async Task<IActionResult> SeedImages()
    {
        try
        {
            logger.LogInformation("Starting product image seeding...");

            var products = await context.Products.ToListAsync();
            if (!products.Any())
            {
                return BadRequest("No products found. Please run the main seeder first.");
            }

            var seedImages = GetSeedImageFiles();
            if (!seedImages.Any())
            {
                return BadRequest("No seed images found.");
            }

            var totalImagesUploaded = 0;

            foreach (var product in products)
            {
                try
                {
                    // Each product gets exactly 2 images
                    var productImages = new List<ProductImage>();
                    
                    for (int i = 0; i < 2; i++)
                    {
                        var imagePath = seedImages[i % seedImages.Count];
                        var fileName = Path.GetFileName(imagePath);

                        logger.LogInformation($"Uploading image {fileName} for product {product.Name}...");

                        using var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                        var imageType = i == 0 ? ImageType.Main : ImageType.Gallery;

                        var uploadResult = await cloudinaryService.UploadImageAsync(
                            fileStream,
                            fileName,
                            imageType,
                            $"{product.Name} - Image {i + 1}",
                            CancellationToken.None);

                        if (uploadResult.IsSuccessful)
                        {
                            var productImage = ProductImage.Create(
                                product.Id,
                                uploadResult.PublicId,
                                uploadResult.SecureUrl,
                                uploadResult.ThumbnailUrl,
                                uploadResult.LargeUrl,
                                i + 1,
                                imageType,
                                uploadResult.FileSizeBytes,
                                $"{product.Name} - Image {i + 1}");

                            productImages.Add(productImage);
                            totalImagesUploaded++;
                        }
                        else
                        {
                            logger.LogWarning($"Failed to upload image {fileName} for product {product.Name}: {uploadResult.ErrorMessage}");
                        }
                    }

                    if (productImages.Any())
                    {
                        await context.ProductImages.AddRangeAsync(productImages);
                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error uploading images for product {product.Name}");
                }
            }

            logger.LogInformation($"Image seeding completed. Total images uploaded: {totalImagesUploaded}");
            return Ok($"Successfully uploaded {totalImagesUploaded} images for {products.Count} products.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in image seeding process");
            return StatusCode(500, "An error occurred during image seeding.");
        }
    }

    private List<string> GetSeedImageFiles()
    {
        try
        {
            // Try multiple possible paths
            var possiblePaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "src", "Infrastructure", "ECommerce.Persistence", "SeedImages"),
                Path.Combine(Directory.GetCurrentDirectory(), "SeedImages"),
                "/Users/emre/Desktop/projeler/ecommerce-proj/ECommerce/src/Infrastructure/ECommerce.Persistence/SeedImages"
            };

            string? seedImagesDirectory = null;
            foreach (var path in possiblePaths)
            {
                logger.LogInformation($"Checking path: {path}");
                if (Directory.Exists(path))
                {
                    seedImagesDirectory = path;
                    break;
                }
            }

            if (string.IsNullOrEmpty(seedImagesDirectory))
            {
                logger.LogWarning($"Seed images directory not found. Tried paths: {string.Join(", ", possiblePaths)}");
                return new List<string>();
            }

            var supportedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var imageFiles = Directory.GetFiles(seedImagesDirectory)
                .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .ToList();

            logger.LogInformation($"Found {imageFiles.Count} seed images in {seedImagesDirectory}");
            return imageFiles;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting seed image files");
            return new List<string>();
        }
    }
} 