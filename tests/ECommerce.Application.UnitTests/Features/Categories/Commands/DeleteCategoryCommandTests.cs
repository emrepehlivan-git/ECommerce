using Ardalis.Result;
using ECommerce.Application.Features.Categories;
using ECommerce.Application.Features.Categories.Commands;
using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.DependencyInjection;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;

namespace ECommerce.Application.UnitTests.Features.Categories.Commands;

public sealed class DeleteCategoryCommandTests : CategoryCommandsTestBase
{
    private readonly DeleteCategoryCommandHandler Handler;
    private readonly DeleteCategoryCommand Command;
    private readonly Guid CategoryId = new("e150053f-2c4c-4c8a-a1ea-d83b7ba89d1a");

    public DeleteCategoryCommandTests()
    {
        Command = new DeleteCategoryCommand(CategoryId);

        Handler = new DeleteCategoryCommandHandler(
            CategoryRepositoryMock.Object,
            CacheManagerMock.Object,
            LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteCategory()
    {
        // Arrange
        var existingCategory = Category.Create("Test Category");
        SetupCategoryRepositoryGetByIdAsync(existingCategory);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldReturnNotFound()
    {
        // Arrange
        SetupCategoryRepositoryGetByIdAsync(null);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WithCategoryHavingProducts_ShouldReturnConflict()
    {
        // Arrange
        var existingCategory = Category.Create("Test Category");
        var product = Product.Create("Test Product", "Description", 100, CategoryId, 10);
        existingCategory.Products.Add(product);
        SetupCategoryRepositoryGetByIdAsync(existingCategory);
        CategoryRepositoryMock
            .Setup(x => x.HasProductsAsync(CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Conflict);
    }

    [Fact]
    public async Task Handle_WithValidCategory_ShouldIncludeProductsInQuery()
    {
        // Arrange
        var existingCategory = Category.Create("Test Category");
        var product = Product.Create("Test Product", "Description", 100, CategoryId, 10);
        existingCategory.Products.Add(product);

        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(
                CategoryId,
                It.IsAny<Expression<Func<IQueryable<Category>, IQueryable<Category>>>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategory);

        CategoryRepositoryMock
            .Setup(x => x.HasProductsAsync(CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Conflict);

        // Verify that the category was queried with products included
        CategoryRepositoryMock.Verify(
            x => x.GetByIdAsync(
                CategoryId,
                It.IsAny<Expression<Func<IQueryable<Category>, IQueryable<Category>>>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
