using Bogus;
using ECommerce.Application.Constants;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.ValueObjects;
using ECommerce.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Persistence.Seeds;

public sealed class DatabaseSeeder(
    ApplicationDbContext context,
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    Application.Common.Logging.IECommerLogger<DatabaseSeeder> logger)
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<User> _userManager = userManager;
    private readonly RoleManager<Role> _roleManager = roleManager;
    private readonly Application.Common.Logging.IECommerLogger<DatabaseSeeder> _logger = logger;

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();

            // Seed in order due to dependencies
            await SeedPermissionsAsync();
            await SeedRolesAsync();
            await SeedUsersAsync();
            await SeedCategoriesAsync();
            await SeedProductsAsync();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedPermissionsAsync()
    {
        if (await _context.Permissions.AnyAsync())
        {
            _logger.LogInformation("Permissions already exist. Skipping permissions seeding.");
            return;
        }

        _logger.LogInformation("Seeding permissions...");

        var permissions = new List<Permission>
        {
            // User permissions
            Permission.Create(PermissionConstants.Users.View, "View users", "Users", "View"),
            Permission.Create(PermissionConstants.Users.Create, "Create users", "Users", "Create"),
            Permission.Create(PermissionConstants.Users.Update, "Update users", "Users", "Update"),
            Permission.Create(PermissionConstants.Users.Delete, "Delete users", "Users", "Delete"),
            Permission.Create(PermissionConstants.Users.Manage, "Manage users", "Users", "Manage"),

            // Product permissions
            Permission.Create(PermissionConstants.Products.Create, "Create products", "Products", "Create"),
            Permission.Create(PermissionConstants.Products.Update, "Update products", "Products", "Update"),
            Permission.Create(PermissionConstants.Products.Delete, "Delete products", "Products", "Delete"),
            Permission.Create(PermissionConstants.Products.Manage, "Manage products", "Products", "Manage"),

            // Category permissions
            Permission.Create(PermissionConstants.Categories.Create, "Create categories", "Categories", "Create"),
            Permission.Create(PermissionConstants.Categories.Update, "Update categories", "Categories", "Update"),
            Permission.Create(PermissionConstants.Categories.Delete, "Delete categories", "Categories", "Delete"),
            Permission.Create(PermissionConstants.Categories.Manage, "Manage categories", "Categories", "Manage"),

            // Order permissions
            Permission.Create(PermissionConstants.Orders.View, "View orders", "Orders", "View"),
            Permission.Create(PermissionConstants.Orders.Create, "Create orders", "Orders", "Create"),
            Permission.Create(PermissionConstants.Orders.Update, "Update orders", "Orders", "Update"),
            Permission.Create(PermissionConstants.Orders.Delete, "Delete orders", "Orders", "Delete"),
            Permission.Create(PermissionConstants.Orders.Manage, "Manage orders", "Orders", "Manage"),

            // Role permissions
            Permission.Create(PermissionConstants.Roles.View, "View roles", "Roles", "View"),
            Permission.Create(PermissionConstants.Roles.Create, "Create roles", "Roles", "Create"),
            Permission.Create(PermissionConstants.Roles.Update, "Update roles", "Roles", "Update"),
            Permission.Create(PermissionConstants.Roles.Delete, "Delete roles", "Roles", "Delete"),
            Permission.Create(PermissionConstants.Roles.Manage, "Manage roles", "Roles", "Manage"),
        };

        await _context.Permissions.AddRangeAsync(permissions);
        await _context.SaveChangesAsync();
    }

    private async Task SeedRolesAsync()
    {
        // Check if custom roles already exist (excluding default Identity roles)
        var customRoles = await _roleManager.Roles
            .Where(r => r.NormalizedName == "ADMIN" || r.NormalizedName == "CUSTOMER" || r.NormalizedName == "MANAGER")
            .AnyAsync();

        if (customRoles)
        {
            _logger.LogInformation("Custom roles already exist. Skipping roles seeding.");
            return;
        }

        _logger.LogInformation("Seeding roles...");

        var permissions = await _context.Permissions.ToListAsync();

        // Admin role with all permissions - check if ADMIN role already exists
        var existingAdminRole = await _roleManager.FindByNameAsync("ADMIN");
        Role adminRole;
        if (existingAdminRole != null)
        {
            adminRole = existingAdminRole;
        }
        else
        {
            adminRole = Role.Create("Admin");
            await _roleManager.CreateAsync(adminRole);
        }

        // Add permissions to admin role if not already added
        var existingAdminPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == adminRole.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        foreach (var permission in permissions)
        {
            if (!existingAdminPermissions.Contains(permission.Id))
            {
                var rolePermission = RolePermission.Create(adminRole, permission);
                await _context.RolePermissions.AddAsync(rolePermission);
            }
        }

        // Customer role with limited permissions
        var customerRole = Role.Create("Customer");
        await _roleManager.CreateAsync(customerRole);

        var customerPermissions = permissions.Where(p => 
            p.Name == PermissionConstants.Orders.View ||
            p.Name == PermissionConstants.Orders.Create).ToList();

        foreach (var permission in customerPermissions)
        {
            var rolePermission = RolePermission.Create(customerRole, permission);
            await _context.RolePermissions.AddAsync(rolePermission);
        }

        // Manager role with product and stock permissions
        var managerRole = Role.Create("Manager");
        await _roleManager.CreateAsync(managerRole);

        var managerPermissions = permissions.Where(p => 
            p.Module == "Products" || 
            p.Module == "Categories" || 
            p.Name == PermissionConstants.Orders.View ||
            p.Name == PermissionConstants.Orders.Manage).ToList();

        foreach (var permission in managerPermissions)
        {
            var rolePermission = RolePermission.Create(managerRole, permission);
            await _context.RolePermissions.AddAsync(rolePermission);
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedUsersAsync()
    {
        // Check if seed users already exist
        var seedUsers = await _userManager.Users
            .Where(u => u.Email == "admin@ecommerce.com" || 
                       u.Email == "manager@ecommerce.com")
            .AnyAsync();

        if (seedUsers)
        {
            _logger.LogInformation("Seed users already exist. Skipping users seeding.");
            return;
        }

        _logger.LogInformation("Seeding users...");

        var roles = await _roleManager.Roles.ToListAsync();
        
        // Find admin role (could be "Admin" or "ADMIN")
        var adminRole = roles.FirstOrDefault(r => r.NormalizedName == "ADMIN") ?? 
                       roles.FirstOrDefault(r => r.Name == "Admin");
        
        // Find or create customer role
        var customerRole = roles.FirstOrDefault(r => r.NormalizedName == "CUSTOMER") ?? 
                          roles.FirstOrDefault(r => r.Name == "Customer");
        
        if (customerRole == null)
        {
            customerRole = Role.Create("Customer");
            await _roleManager.CreateAsync(customerRole);
        }
        
        // Find or create manager role
        var managerRole = roles.FirstOrDefault(r => r.NormalizedName == "MANAGER") ?? 
                         roles.FirstOrDefault(r => r.Name == "Manager");
        
        if (managerRole == null)
        {
            managerRole = Role.Create("Manager");
            await _roleManager.CreateAsync(managerRole);
        }

        // Create admin user if admin role exists
        if (adminRole != null)
        {
            var adminUser = User.Create("admin@ecommerce.com", "Admin", "User");
            var adminResult = await _userManager.CreateAsync(adminUser, "Admin123!");
            if (adminResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, adminRole.Name!);
            }
        }

        // Create manager user
        var managerUser = User.Create("manager@ecommerce.com", "Manager", "User");
        var managerResult = await _userManager.CreateAsync(managerUser, "Manager123!");
        if (managerResult.Succeeded)
        {
            await _userManager.AddToRoleAsync(managerUser, managerRole.Name!);
        }

        // Create fake customer users using Bogus
        var userFaker = new Faker<User>()
            .CustomInstantiator(f => User.Create(
                f.Internet.Email(),
                f.Name.FirstName(),
                f.Name.LastName()));

        var customers = userFaker.Generate(20);

        foreach (var customer in customers)
        {
            var result = await _userManager.CreateAsync(customer, "Customer123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(customer, customerRole.Name!);
            }
        }
    }

    private async Task SeedCategoriesAsync()
    {
        if (await _context.Categories.AnyAsync())
        {
            _logger.LogInformation("Categories already exist. Skipping categories seeding.");
            return;
        }

        _logger.LogInformation("Seeding categories...");

        var categoryNames = new[]
        {
            "Electronics", "Clothing", "Books", "Home & Garden", "Sports & Outdoors",
            "Health & Beauty", "Toys & Games", "Automotive", "Food & Beverages", "Jewelry"
        };

        var categories = categoryNames.Select(Category.Create).ToList();
        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();
    }

    private async Task SeedProductsAsync()
    {
        if (await _context.Products.AnyAsync())
        {
            _logger.LogInformation("Products already exist. Skipping products seeding.");
            return;
        }

        _logger.LogInformation("Seeding products...");

        var categories = await _context.Categories.ToListAsync();

        if (!categories.Any())
        {
            _logger.LogWarning("No categories found. Skipping products seeding.");
            return;
        }

        var productFaker = new Faker<Product>()
            .CustomInstantiator(f =>
            {
                var category = f.PickRandom(categories);
                return Product.Create(
                    f.Commerce.ProductName(),
                    f.Commerce.ProductDescription(),
                    decimal.Parse(f.Commerce.Price(10, 1000)),
                    category.Id,
                    f.Random.Int(0, 100));
            });

        var products = productFaker.Generate(100);
        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();
    }
} 
