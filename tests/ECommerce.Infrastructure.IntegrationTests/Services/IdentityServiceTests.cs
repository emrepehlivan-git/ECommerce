using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace ECommerce.Infrastructure.IntegrationTests.Services;

public class IdentityServiceTests : IDisposable
{
    private readonly IdentityService _identityService;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ApplicationDbContext _context;

    public IdentityServiceTests()
    {
        var services = new ServiceCollection();
        
        // Setup test database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        // Setup Identity
        services.AddIdentity<User, Role>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 1;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddLogging();
        services.AddScoped<IdentityService>();

        var serviceProvider = services.BuildServiceProvider();
        
        _context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        _userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        _roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
        _signInManager = serviceProvider.GetRequiredService<SignInManager<User>>();
        _identityService = serviceProvider.GetRequiredService<IdentityService>();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateUser_WhenValidUserAndPassword()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User");
        var password = "TestPassword123";

        // Act
        var result = await _identityService.CreateAsync(user, password);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        var createdUser = await _identityService.FindByEmailAsync("test@example.com");
        createdUser.Should().NotBeNull();
        createdUser!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task FindByEmailAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var user = User.Create("find@example.com", "Find", "User");
        await _identityService.CreateAsync(user, "password");

        // Act
        var result = await _identityService.FindByEmailAsync("find@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("find@example.com");
        result.FullName.FirstName.Should().Be("Find");
        result.FullName.LastName.Should().Be("User");
    }

    [Fact]
    public async Task FindByEmailAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Act
        var result = await _identityService.FindByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var user = User.Create("findbyid@example.com", "FindById", "User");
        await _identityService.CreateAsync(user, "password");
        
        // Act
        var result = await _identityService.FindByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be("findbyid@example.com");
    }

    [Fact]
    public async Task CheckPasswordAsync_ShouldReturnTrue_WhenPasswordIsCorrect()
    {
        // Arrange
        var user = User.Create("checkpass@example.com", "Check", "Pass");
        var password = "CorrectPassword123";
        await _identityService.CreateAsync(user, password);

        // Act
        var result = await _identityService.CheckPasswordAsync(user, password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckPasswordAsync_ShouldReturnFalse_WhenPasswordIsIncorrect()
    {
        // Arrange
        var user = User.Create("wrongpass@example.com", "Wrong", "Pass");
        await _identityService.CreateAsync(user, "CorrectPassword123");

        // Act
        var result = await _identityService.CheckPasswordAsync(user, "WrongPassword");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CreateRoleAsync_ShouldCreateRole_WhenValidRole()
    {
        // Arrange
        var role = Role.Create("TestRole");

        // Act
        var result = await _identityService.CreateRoleAsync(role);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        var createdRole = await _identityService.FindRoleByNameAsync("TestRole");
        createdRole.Should().NotBeNull();
        createdRole!.Name.Should().Be("TestRole");
    }

    [Fact]
    public async Task RoleExistsAsync_ShouldReturnTrue_WhenRoleExists()
    {
        // Arrange
        var role = Role.Create("ExistingRole");
        await _identityService.CreateRoleAsync(role);

        // Act
        var result = await _identityService.RoleExistsAsync("ExistingRole");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RoleExistsAsync_ShouldReturnFalse_WhenRoleDoesNotExist()
    {
        // Act
        var result = await _identityService.RoleExistsAsync("NonExistentRole");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddToRoleAsync_ShouldAddUserToRole_WhenValidUserAndRole()
    {
        // Arrange
        var user = User.Create("userrole@example.com", "User", "Role");
        var role = Role.Create("UserRole");
        
        await _identityService.CreateAsync(user, "password");
        await _identityService.CreateRoleAsync(role);

        // Act
        var result = await _identityService.AddToRoleAsync(user, "UserRole");

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        var userRoles = await _identityService.GetUserRolesAsync(user);
        userRoles.Should().Contain("UserRole");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser_WhenValidUser()
    {
        // Arrange
        var user = User.Create("update@example.com", "Update", "User");
        await _identityService.CreateAsync(user, "password");
        
        user.UpdateName("Updated", "Name");

        // Act
        var result = await _identityService.UpdateAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        var updatedUser = await _identityService.FindByIdAsync(user.Id);
        updatedUser!.FullName.FirstName.Should().Be("Updated");
        updatedUser.FullName.LastName.Should().Be("Name");
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
} 