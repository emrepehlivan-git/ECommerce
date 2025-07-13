using ECommerce.Application.Common.Logging;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.ValueObjects;
using ECommerce.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Persistence;

public class DataSeeder(ApplicationDbContext context, IECommerceLogger<DataSeeder> logger)
{
    private readonly Random Random = new Random(42);
    private readonly string[] SeedImageFiles =
        [
            "c-d-x-PDX_a_82obo-unsplash.jpg",
            "daniel-korpai-hbTKIbuMmBI-unsplash.jpg",
            "domino-studio-164_6wVEHfI-unsplash.jpg",
            "eniko-kis-KsLPTsYaqIQ-unsplash.jpg",
            "jakob-owens-O_bhy3TnSYU-unsplash.jpg",
            "joan-tran-reEySFadyJQ-unsplash.jpg",
            "rachit-tank-2cFZ_FB08UM-unsplash.jpg"
        ];

    public async Task SeedAllAsync()
    {
        logger.LogInformation("Starting data seeding process...");

        try
        {
            var categoryCount = await context.Categories.CountAsync();
            var productCount = await context.Products.CountAsync();

            if (categoryCount > 0 || productCount > 0)
            {
                logger.LogInformation("Data already exists. Skipping seeding process.");
                return;
            }

            var categories = await SeedCategoriesAsync();
            var products = await SeedProductsAsync(categories);
            await SeedProductStockAsync(products);
            await SeedProductImagesAsync(products);

            await context.SaveChangesAsync();
            
            logger.LogInformation("Data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during data seeding");
            throw;
        }
    }

    private async Task<List<Category>> SeedCategoriesAsync()
    {
        logger.LogInformation("Seeding 200 categories...");

        var categoryNames = GenerateCategoryNames();
        var categories = new List<Category>();

        foreach (var name in categoryNames)
        {
            var category = Category.Create(name);
            categories.Add(category);
        }

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        logger.LogInformation("Successfully seeded {Count} categories", categories.Count);
        return categories;
    }

    private async Task<List<Product>> SeedProductsAsync(List<Category> categories)
    {
        logger.LogInformation("Seeding 10,000 products...");

        var products = new List<Product>();
        var productNames = GenerateProductNames();

        for (int i = 0; i < 10000; i++)
        {
            var category = categories[Random.Next(categories.Count)];
            var name = productNames[i % productNames.Count] + $" {i + 1}";
            var description = GenerateProductDescription(name);
            var price = Price.Create(Random.Next(10, 5000) + (Random.NextDouble() > 0.5 ? 0.99m : 0.49m));
            var initialStock = Random.Next(10, 100);

            var product = Product.Create(name, description, price, category.Id, initialStock);
            products.Add(product);

            if (products.Count % 1000 == 0)
            {
                await context.Products.AddRangeAsync(products.Skip(products.Count - 1000).Take(1000));
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} products so far...", products.Count);
            }
        }

        var remainingProducts = products.Skip((products.Count / 1000) * 1000);
        if (remainingProducts.Any())
        {
            await context.Products.AddRangeAsync(remainingProducts);
            await context.SaveChangesAsync();
        }

        logger.LogInformation("Successfully seeded {Count} products", products.Count);
        return products;
    }

    private async Task SeedProductStockAsync(List<Product> products)
    {
        logger.LogInformation("Seeding product stock for {Count} products...", products.Count);

        var stockEntries = new List<ProductStock>();

        foreach (var product in products)
        {
            var quantity = Random.Next(0, 1000);
            var stock = ProductStock.Create(product.Id, quantity);
            stockEntries.Add(stock);

            if (stockEntries.Count % 1000 == 0)
            {
                await context.ProductStocks.AddRangeAsync(stockEntries.Skip(stockEntries.Count - 1000).Take(1000));
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded stock for {Count} products so far...", stockEntries.Count);
            }
        }

        var remainingStock = stockEntries.Skip((stockEntries.Count / 1000) * 1000);
        if (remainingStock.Any())
        {
            await context.ProductStocks.AddRangeAsync(remainingStock);
            await context.SaveChangesAsync();
        }

        logger.LogInformation("Successfully seeded stock for {Count} products", stockEntries.Count);
    }

    private async Task SeedProductImagesAsync(List<Product> products)
    {
        logger.LogInformation("Seeding product images for {Count} products...", products.Count);

        var productImages = new List<ProductImage>();

        foreach (var product in products)
        {
            var imageCount = Random.Next(1, 5);
            
            for (int i = 0; i < imageCount; i++)
            {
                var imageFile = SeedImageFiles[Random.Next(SeedImageFiles.Length)];
                var publicId = $"ecommerce/products/{product.Id}_{i}_{Path.GetFileNameWithoutExtension(imageFile)}";
                var imageUrl = $"https://res.cloudinary.com/your-cloud-name/image/upload/{publicId}.jpg";
                var thumbnailUrl = $"https://res.cloudinary.com/your-cloud-name/image/upload/w_150,h_150,c_fill/{publicId}.jpg";
                var largeUrl = $"https://res.cloudinary.com/your-cloud-name/image/upload/w_800,h_800,c_fill/{publicId}.jpg";
                var imageType = i == 0 ? ImageType.Main : ImageType.Gallery;
                var fileSizeBytes = Random.Next(50000, 500000); // 50KB to 500KB
                var altText = $"{product.Name} - Image {i + 1}";

                var productImage = ProductImage.Create(
                    product.Id,
                    publicId,
                    imageUrl,
                    thumbnailUrl,
                    largeUrl,
                    i,
                    imageType,
                    fileSizeBytes,
                    altText);

                productImages.Add(productImage);
            }

            if (productImages.Count >= 1000)
            {
                await context.ProductImages.AddRangeAsync(productImages.Take(1000));
                await context.SaveChangesAsync();
                productImages.RemoveRange(0, 1000);
                logger.LogInformation("Seeded images batch...");
            }
        }

        if (productImages.Count != 0)
        {
            await context.ProductImages.AddRangeAsync(productImages);
            await context.SaveChangesAsync();
        }

        logger.LogInformation("Successfully seeded product images");
    }

    private List<string> GenerateCategoryNames()
    {
        var categories = new List<string>
        {
            "Smartphones", "Laptops", "Tablets", "Desktop Computers", "Gaming Consoles", "Smart Watches", "Headphones",
            "Cameras", "Audio Systems", "Smart Home Devices", "Wearable Technology", "Computer Accessories",
            
            "Men's Clothing", "Women's Clothing", "Children's Clothing", "Shoes", "Bags & Luggage", "Jewelry",
            "Watches", "Sunglasses", "Hats & Caps", "Belts & Accessories", "Underwear & Sleepwear", "Sportswear",
            
            "Furniture", "Home Decor", "Kitchen Appliances", "Bedding & Bath", "Lighting", "Garden Tools",
            "Outdoor Furniture", "Home Security", "Cleaning Supplies", "Storage & Organization", "Home Improvement",
            
            "Skincare", "Makeup", "Hair Care", "Personal Care", "Health Supplements", "Fitness Equipment",
            "Medical Devices", "Oral Care", "Fragrances", "Beauty Tools", "Wellness Products",
            
            "Exercise Equipment", "Outdoor Gear", "Sports Apparel", "Team Sports", "Water Sports", "Winter Sports",
            "Cycling", "Running", "Yoga & Pilates", "Hunting & Fishing", "Camping Equipment", "Athletic Shoes",
            
            "Books", "Movies & TV", "Music", "Video Games", "Educational Materials", "E-books", "Audiobooks",
            "Board Games", "Puzzles", "Magazines", "Comics & Graphic Novels",
            
            "Car Accessories", "Car Electronics", "Automotive Tools", "Car Care Products", "Motorcycle Accessories",
            "Replacement Parts", "Tires & Wheels", "Car Audio", "Navigation Systems", "Car Safety",
            
            "Baby Clothing", "Baby Gear", "Toys", "Educational Toys", "Baby Care", "Children's Books",
            "Baby Furniture", "Strollers", "Car Seats", "Baby Monitors", "Feeding Accessories",
            
            "Organic Foods", "Snacks", "Beverages", "Cooking Ingredients", "Specialty Foods", "International Foods",
            "Health Foods", "Pet Food", "Kitchen Utensils", "Food Storage",
            
            "Office Supplies", "Business Equipment", "Desk Accessories", "Filing & Storage", "Presentation Supplies",
            "Printing & Copying", "Office Furniture", "Business Software", "Office Electronics",
            
            "Dog Supplies", "Cat Supplies", "Bird Supplies", "Fish & Aquarium", "Small Pet Supplies",
            "Pet Grooming", "Pet Toys", "Pet Health", "Pet Beds & Furniture", "Pet Training",

            "Art Supplies", "Craft Materials", "Sewing & Fabric", "Scrapbooking", "Model Building",
            "Painting Supplies", "Drawing Materials", "Crafting Tools", "Hobby Supplies",
            
            "Guitars", "Keyboards & Pianos", "Drums & Percussion", "Wind Instruments", "String Instruments",
            "Music Accessories", "Audio Recording", "DJ Equipment", "Sheet Music",
            
            "Suitcases", "Travel Backpacks", "Travel Accessories", "Travel Electronics", "Travel Comfort",
            "Maps & Guides", "Travel Safety", "Luggage Sets", "Travel Organizers",
            
            "Collectible Cards", "Action Figures", "Model Trains", "Coin Collecting", "Stamp Collecting",
            "Vintage Items", "Memorabilia", "Hobby Tools", "Display Cases",
            
            "Lab Equipment", "Industrial Tools", "Safety Equipment", "Measuring Instruments", "Scientific Supplies",
            "Industrial Electronics", "Commercial Equipment", "Professional Tools"
        };

        return categories.OrderBy(x => Random.Next()).Take(200).ToList();
    }

    private List<string> GenerateProductNames()
    {
        var adjectives = new[]
        {
            "Premium", "Professional", "Advanced", "Ultra", "Pro", "Elite", "Superior", "Deluxe", "Standard",
            "Basic", "Essential", "Compact", "Portable", "Wireless", "Smart", "Digital", "Eco-Friendly",
            "Heavy-Duty", "Lightweight", "Waterproof", "Durable", "Flexible", "Ergonomic", "Multi-Purpose"
        };

        var nouns = new[]
        {
            "Device", "Tool", "Kit", "Set", "System", "Machine", "Gadget", "Equipment", "Accessory",
            "Component", "Unit", "Module", "Panel", "Display", "Controller", "Adapter", "Cable",
            "Stand", "Mount", "Holder", "Case", "Cover", "Protector", "Guard", "Filter", "Sensor",
            "Battery", "Charger", "Power Bank", "Speaker", "Microphone", "Camera", "Monitor",
            "Keyboard", "Mouse", "Headset", "Router", "Switch", "Hub", "Drive", "Memory",
            "Processor", "Card", "Board", "Chip", "Socket", "Connector", "Wire", "Tube"
        };

        var productNames = new List<string>();
        for (int i = 0; i < 100; i++)
        {
            var adj = adjectives[Random.Next(adjectives.Length)];
            var noun = nouns[Random.Next(nouns.Length)];
            productNames.Add($"{adj} {noun}");
        }

        return productNames;
    }

    private string GenerateProductDescription(string productName)
    {
        var templates = new[]
        {
            $"High-quality {productName.ToLower()} designed for optimal performance and reliability.",
            $"Experience the best with our {productName.ToLower()}. Perfect for both personal and professional use.",
            $"Innovative {productName.ToLower()} featuring cutting-edge technology and superior build quality.",
            $"Our {productName.ToLower()} combines functionality with style, making it an excellent choice.",
            $"Discover the power of our {productName.ToLower()}. Built to exceed your expectations.",
            $"Premium {productName.ToLower()} crafted with attention to detail and user satisfaction in mind.",
            $"Reliable and efficient {productName.ToLower()} that delivers exceptional value and performance."
        };

        return templates[Random.Next(templates.Length)];
    }
}