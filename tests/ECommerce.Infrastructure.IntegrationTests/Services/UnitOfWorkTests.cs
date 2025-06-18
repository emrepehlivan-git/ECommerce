using ECommerce.Persistence.Services;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ECommerce.Infrastructure.IntegrationTests.Services;

public class UnitOfWorkTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldSaveChangesToDatabase()
    {
        // Arrange
        var category = Category.Create("Electronics");
        _context.Categories.Add(category);

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        var savedCategory = await _context.Categories.FindAsync(category.Id);
        savedCategory.Should().NotBeNull();
        savedCategory!.Name.Should().Be("Electronics");
    }

    [Fact]
    public void SaveChanges_ShouldSaveChangesToDatabase()
    {
        // Arrange
        var category = Category.Create("Books");
        _context.Categories.Add(category);

        // Act
        var result = _unitOfWork.SaveChanges();

        // Assert
        result.Should().Be(1);
        var savedCategory = _context.Categories.Find(category.Id);
        savedCategory.Should().NotBeNull();
        savedCategory!.Name.Should().Be("Books");
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldReturnZero_WhenNoChanges()
    {
        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void SaveChanges_ShouldReturnZero_WhenNoChanges()
    {
        // Act
        var result = _unitOfWork.SaveChanges();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void BeginTransactionAsync_ShouldHaveMethod()
    {
        // Note: InMemory database doesn't support real transactions
        // This test just verifies the method exists and can be called
        
        // Act & Assert - Just verify the method exists
        var methodInfo = typeof(UnitOfWork).GetMethod(nameof(UnitOfWork.BeginTransactionAsync));
        methodInfo.Should().NotBeNull();
        methodInfo!.ReturnType.Should().Be(typeof(Task<IDbContextTransaction>));
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldSaveMultipleEntities()
    {
        // Arrange
        var category1 = Category.Create("Clothing");
        var category2 = Category.Create("Home");
        var category3 = Category.Create("Garden");

        _context.Categories.AddRange(category1, category2, category3);

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(3);
        var allCategories = _context.Categories.ToList();
        allCategories.Should().HaveCount(3);
        allCategories.Should().Contain(c => c.Name == "Clothing");
        allCategories.Should().Contain(c => c.Name == "Home");
        allCategories.Should().Contain(c => c.Name == "Garden");
    }

    [Fact]
    public async Task SaveChangesAsync_WithCancellationToken_ShouldWork()
    {
        // Arrange
        var category = Category.Create("Automotive");
        _context.Categories.Add(category);
        var cancellationToken = new CancellationToken();

        // Act
        var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Assert
        result.Should().Be(1);
        var savedCategory = await _context.Categories.FindAsync(category.Id);
        savedCategory.Should().NotBeNull();
        savedCategory!.Name.Should().Be("Automotive");
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
} 