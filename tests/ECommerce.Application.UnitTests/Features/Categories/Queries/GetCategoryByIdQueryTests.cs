using ECommerce.Application.Behaviors;
using ECommerce.Application.Features.Categories;
using ECommerce.Application.Features.Categories.Queries;

namespace ECommerce.Application.UnitTests.Features.Categories.Queries;

public sealed class GetCategoryByIdQueryTests : CategoryQueriesTestBase
{
    private readonly GetCategoryByIdQueryHandler Handler;
    private readonly GetCategoryByIdQuery Query;
    private readonly Guid CategoryId = new("e150053f-2c4c-4c8a-a1ea-d83b7ba89d1a");

    public GetCategoryByIdQueryTests()
    {
        Query = new GetCategoryByIdQuery(CategoryId);

        Handler = new GetCategoryByIdQueryHandler(
            CategoryRepositoryMock.Object,
            LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldReturnCategory()
    {
        // Arrange
        var category = Category.Create("Test Category");
        SetupCategoryRepositoryGetByIdAsync(category);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(category.Id);
        result.Value.Name.Should().Be(category.Name);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        SetupCategoryRepositoryGetByIdAsync(null);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(Localizer[CategoryConsts.NotFound]);
    }

    [Fact]
    public async Task Handle_WithIncludeProducts_ShouldReturnCategoryWithProducts()
    {
        // Arrange
        var category = Category.Create("Test Category");
        var product = Product.Create("Test Product", "Description", 100m, category.Id, 10);
        category.Products = [product];

        SetupCategoryRepositoryGetByIdAsync(category);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void Query_ShouldImplementICacheableRequest()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetCategoryByIdQuery(categoryId);

        // Act & Assert
        query.Should().BeAssignableTo<ICacheableRequest>();
        query.CacheKey.Should().Be($"category:{categoryId}");
        query.CacheDuration.Should().Be(TimeSpan.FromHours(2));
    }
}