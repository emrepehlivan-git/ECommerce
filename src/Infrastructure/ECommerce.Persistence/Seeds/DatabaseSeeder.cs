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

    private static readonly Lazy<List<(string PermissionName, string Module, string Action)>> _cachedPermissions = 
        new(() => GetAllPermissionsFromConstants());

    private static List<(string PermissionName, string Module, string Action)> CachedPermissions => 
        _cachedPermissions.Value;

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            await _context.Database.EnsureCreatedAsync();

            await SeedPermissionsAsync();
        await SeedRolesAsync();
        await EnsureAdminHasAllPermissionsAsync(); 
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
        _logger.LogInformation("Seeding permissions...");

        var permissionDefinitions = CachedPermissions;
        var existingPermissions = await _context.Permissions
            .Select(p => p.Name)
            .ToListAsync();

        var permissionsToAdd = new List<Permission>();

        foreach (var (permissionName, module, action) in permissionDefinitions)
        {
            if (!existingPermissions.Contains(permissionName))
            {
                var description = GeneratePermissionDescription(action, module);
                var permission = Permission.Create(permissionName, description, module, action);
                permissionsToAdd.Add(permission);
                _logger.LogInformation("Adding new permission: {PermissionName}", permissionName);
            }
        }

        if (permissionsToAdd.Count > 0)
        {
            await _context.Permissions.AddRangeAsync(permissionsToAdd);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Added {Count} new permissions", permissionsToAdd.Count);
        }
        else
        {
            _logger.LogInformation("No new permissions to add");
        }
    }

    private static List<(string PermissionName, string Module, string Action)> GetAllPermissionsFromConstants()
    {
        var permissions = new List<(string, string, string)>();
        var permissionTypes = typeof(PermissionConstants).GetNestedTypes();

        foreach (var type in permissionTypes)
        {
            var moduleName = type.Name; 
            var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(string))
                {
                    var permissionValue = (string)field.GetValue(null)!;
                    var actionName = field.Name; 
                    permissions.Add((permissionValue, moduleName, actionName));
                }
            }
        }

        return permissions;
    }

    private static string GeneratePermissionDescription(string action, string module)
    {
        return action.ToLower() switch
        {
            "read" => $"Read {module.ToLower()}",
            "view" => $"View {module.ToLower()}",
            "create" => $"Create {module.ToLower()}",
            "update" => $"Update {module.ToLower()}",
            "delete" => $"Delete {module.ToLower()}",
            "manage" => $"Manage {module.ToLower()}",
            _ => $"{action} {module.ToLower()}"
        };
    }

    private async Task EnsureAdminHasAllPermissionsAsync()
    {
        _logger.LogInformation("Ensuring Admin has all permissions...");

        var adminRole = await _roleManager.FindByNameAsync("Admin");

        if (adminRole == null)
        {
            _logger.LogWarning("Admin role not found. Skipping admin permission assignment.");
            return;
        }

        var allPermissions = await _context.Permissions.ToListAsync();
        
        var existingAdminPermissionIdsList = await _context.RolePermissions
            .Where(rp => rp.RoleId == adminRole.Id && rp.IsActive)
            .Select(rp => rp.PermissionId)
            .ToListAsync();
        
        var existingAdminPermissionIds = existingAdminPermissionIdsList.ToHashSet(); 

        var missingPermissions = allPermissions
            .Where(p => !existingAdminPermissionIds.Contains(p.Id))
            .ToList();

        if (missingPermissions.Count == 0)
        {
            _logger.LogInformation("Admin already has all permissions");
            return;
        }

        var newRolePermissions = missingPermissions
            .Select(permission => RolePermission.Create(adminRole, permission))
            .ToList();

        await _context.RolePermissions.AddRangeAsync(newRolePermissions);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Added {Count} missing permissions to Admin role", missingPermissions.Count);
        
        foreach (var permission in missingPermissions)
        {
            _logger.LogDebug("Added permission {PermissionName} to Admin role", permission.Name);
        }
    }

    private async Task SeedRolesAsync()
    {
        _logger.LogInformation("Seeding roles...");

        var permissions = await _context.Permissions.ToListAsync();

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
            _logger.LogInformation("Created Admin role");
        }

        var existingCustomerRole = await _roleManager.FindByNameAsync("CUSTOMER");
        Role customerRole;
        if (existingCustomerRole != null)
        {
            customerRole = existingCustomerRole;
        }
        else
        {
            customerRole = Role.Create("Customer");
            await _roleManager.CreateAsync(customerRole);
            _logger.LogInformation("Created Customer role");
        }

        var customerPermissionNames = new[] { PermissionConstants.Orders.View, PermissionConstants.Orders.Create };
        var customerPermissions = permissions.Where(p => customerPermissionNames.Contains(p.Name)).ToList();

        var existingCustomerPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == customerRole.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        var newCustomerPermissions = new List<RolePermission>();
        foreach (var permission in customerPermissions)
        {
            if (!existingCustomerPermissions.Contains(permission.Id))
            {
                var rolePermission = RolePermission.Create(customerRole, permission);
                newCustomerPermissions.Add(rolePermission);
                _logger.LogInformation("Adding permission {PermissionName} to Customer role", permission.Name);
            }
        }

        if (newCustomerPermissions.Count > 0)
        {
            await _context.RolePermissions.AddRangeAsync(newCustomerPermissions);
        }

        var existingManagerRole = await _roleManager.FindByNameAsync("MANAGER");
        Role managerRole;
        if (existingManagerRole != null)
        {
            managerRole = existingManagerRole;
        }
        else
        {
            managerRole = Role.Create("Manager");
            await _roleManager.CreateAsync(managerRole);
            _logger.LogInformation("Created Manager role");
        }

        var managerPermissions = permissions.Where(p => 
            p.Module == "Products" || 
            p.Module == "Categories" || 
            p.Name == PermissionConstants.Orders.View ||
            p.Name == PermissionConstants.Orders.Manage).ToList();

        var existingManagerPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == managerRole.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        var newManagerPermissions = new List<RolePermission>();
        foreach (var permission in managerPermissions)
        {
            if (!existingManagerPermissions.Contains(permission.Id))
            {
                var rolePermission = RolePermission.Create(managerRole, permission);
                newManagerPermissions.Add(rolePermission);
                _logger.LogInformation("Adding permission {PermissionName} to Manager role", permission.Name);
            }
        }

        if (newManagerPermissions.Count > 0)
        {
            await _context.RolePermissions.AddRangeAsync(newManagerPermissions);
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedUsersAsync()
    {
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
        
        var adminRole = roles.FirstOrDefault(r => r.NormalizedName == "ADMIN") ?? 
                       roles.FirstOrDefault(r => r.Name == "Admin");
        
        var customerRole = roles.FirstOrDefault(r => r.NormalizedName == "CUSTOMER") ?? 
                          roles.FirstOrDefault(r => r.Name == "Customer");
        
        if (customerRole == null)
        {
            customerRole = Role.Create("Customer");
            await _roleManager.CreateAsync(customerRole);
        }

        var managerRole = roles.FirstOrDefault(r => r.Name == "Manager");
        
        if (managerRole == null)
        {
            managerRole = Role.Create("Manager");
            await _roleManager.CreateAsync(managerRole);
        }

        if (adminRole != null)
        {
            var adminUser = User.Create("admin@ecommerce.com", "Admin", "User");
            var adminResult = await _userManager.CreateAsync(adminUser, "Admin123!");
            if (adminResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, adminRole.Name!);
            }
        }

        var managerUser = User.Create("manager@ecommerce.com", "Manager", "User");
        var managerResult = await _userManager.CreateAsync(managerUser, "Manager123!");
        if (managerResult.Succeeded)
        {
            await _userManager.AddToRoleAsync(managerUser, managerRole.Name!);
        }

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

    public async Task EnsureAdminHasAllPermissionsPublicAsync()
    {
        await EnsureAdminHasAllPermissionsAsync();
    }

    public static List<(string PermissionName, string Module, string Action)> GetAllPermissionConstants()
        => CachedPermissions;
} 
