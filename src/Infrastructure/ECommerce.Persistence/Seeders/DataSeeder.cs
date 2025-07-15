using Bogus;
using ECommerce.Application.Common.Logging;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.ValueObjects;
using ECommerce.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Persistence.Seeders;

public class DataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IECommerceLogger<DataSeeder> _logger;
    private readonly string _seedImagesPath;

    public DataSeeder(
        ApplicationDbContext context,
        ICloudinaryService cloudinaryService,
        IECommerceLogger<DataSeeder> logger)
    {
        _context = context;
        _cloudinaryService = cloudinaryService;
        _logger = logger;
        _seedImagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedImages");
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting data seeding process...");

            await DeleteAllImagesAsync();
            await SeedCategoriesAsync();
            await SeedProductsAsync();

            _logger.LogInformation("Data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during data seeding");
            throw;
        }
    }

    private async Task DeleteAllImagesAsync()
    {
        try
        {
            _logger.LogInformation("Deleting all existing images from Cloudinary...");
            await _cloudinaryService.DeleteAllImagesAsync();
            _logger.LogInformation("All existing images deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting existing images");
            throw;
        }
    }

    private async Task SeedCategoriesAsync()
    {
        if (await _context.Categories.AnyAsync())
        {
            _logger.LogInformation("Categories already exist, skipping category seeding.");
            return;
        }

        _logger.LogInformation("Seeding categories...");

        var categoryNames = new List<string>
        {
            "Electronics", "Books", "Clothing", "Sports", "Home & Kitchen", "Beauty", "Toys", "Health",
            "Automotive", "Garden", "Music", "Movies", "Video Games", "Jewelry", "Shoes", "Watches",
            "Art", "Crafts", "Industrial", "Office", "Pet Supplies", "Food", "Furniture", "Appliances",
            "Computers", "Phones", "Cameras", "Audio", "Tools", "Hardware", "Software", "Accessories",
            "Bags", "Sunglasses", "Perfumes", "Skincare", "Makeup", "Haircare", "Supplements", "Fitness",
            "Outdoor", "Travel", "Luggage", "Baby", "Kids", "Maternity", "Wedding", "Party", "Gifts", "Collectibles"
        };

        var categories = new List<Category>();
        for (int i = 0; i < 50; i++)
        {
            var categoryName = categoryNames[i % categoryNames.Count];
            if (i >= categoryNames.Count)
            {
                categoryName = $"{categoryName} {i / categoryNames.Count + 1}";
            }
            categories.Add(Category.Create(categoryName));
        }

        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Successfully seeded {categories.Count} categories.");
    }

    private async Task SeedProductsAsync()
    {
        if (await _context.Products.AnyAsync())
        {
            _logger.LogInformation("Products already exist, skipping product seeding.");
            return;
        }

        _logger.LogInformation("Seeding products...");

        var categories = await _context.Categories.ToListAsync();
        var seedImages = GetSeedImageFiles();

        if (seedImages.Count == 0)
        {
            _logger.LogWarning("No seed images found. Products will be created without images.");
        }
        var initialStock = new Random().Next(1, 1000);

        var productFaker = new Faker<Product>()
            .CustomInstantiator(f => Product.Create(
                f.Commerce.ProductName(),
                f.Commerce.ProductDescription(),
                Price.Create(f.Random.Decimal(5, 1000)),
                f.PickRandom(categories).Id,
                initialStock));

        var products = productFaker.Generate(100);

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        var productStocks = products.Select(p => ProductStock.Create(p.Id, initialStock)).ToList();
        await _context.ProductStocks.AddRangeAsync(productStocks);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Successfully seeded {products.Count} products with stocks.");

        if (seedImages.Count > 0)
        {
            await UploadProductImagesAsync(products, seedImages);
        }
    }

    private async Task UploadProductImagesAsync(List<Product> products, List<string> seedImages)
    {
        try
        {
            _logger.LogInformation("Starting image upload process...");

            var random = new Random();
            var totalImagesUploaded = 0;

            foreach (var product in products)
            {
                try
                {
                    var imageCount = random.Next(1, Math.Min(4, seedImages.Count + 1));
                    var productImages = new List<ProductImage>();

                    for (int i = 0; i < imageCount; i++)
                    {
                        var imagePath = seedImages[i % seedImages.Count];
                        var fileName = Path.GetFileName(imagePath);

                        _logger.LogInformation($"Uploading image {fileName} for product {product.Name}...");

                        using var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                        var imageType = i == 0 ? ImageType.Main : ImageType.Gallery;

                        var uploadResult = await _cloudinaryService.UploadImageAsync(
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
                            _logger.LogInformation($"Successfully uploaded image {fileName} for product {product.Name}");
                        }
                        else
                        {
                            _logger.LogWarning($"Failed to upload image {fileName} for product {product.Name}: {uploadResult.ErrorMessage}");
                        }
                    }

                    if (productImages.Any())
                    {
                        await _context.ProductImages.AddRangeAsync(productImages);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Saved {productImages.Count} images for product {product.Name}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error uploading images for product {product.Name}");
                }
            }

            _logger.LogInformation($"Image upload process completed. Total images uploaded: {totalImagesUploaded}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in image upload process");
            throw;
        }
    }

    private List<string> GetSeedImageFiles()
    {
        try
        {
            var seedImagesDirectory = Path.Combine(
                Directory.GetCurrentDirectory(),
                "src",
                "Infrastructure",
                "ECommerce.Persistence",
                "SeedImages");

            if (!Directory.Exists(seedImagesDirectory))
            {
                _logger.LogWarning($"Seed images directory not found: {seedImagesDirectory}");
                return new List<string>();
            }

            var supportedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var imageFiles = Directory.GetFiles(seedImagesDirectory)
                .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .ToList();

            _logger.LogInformation($"Found {imageFiles.Count} seed images in {seedImagesDirectory}");
            return imageFiles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting seed image files");
            return new List<string>();
        }
    }
}