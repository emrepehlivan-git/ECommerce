using ECommerce.Application.Behaviors;
using ECommerce.Application.Features.Categories.V1;

namespace ECommerce.Application.UnitTests.Features.Categories.V1.Queries;

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
        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(CategoryId, It.IsAny<Expression<Func<IQueryable<Category>, IQueryable<Category>>>?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

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
        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(CategoryId, It.IsAny<Expression<Func<IQueryable<Category>, IQueryable<Category>>>?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        SetupDefaultLocalizationMessages();

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(LocalizerMock.Object[CategoryConsts.NotFound]);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectId()
    {
        // Arrange
        var category = Category.Create("Test Category");
        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(CategoryId, It.IsAny<Expression<Func<IQueryable<Category>, IQueryable<Category>>>?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        CategoryRepositoryMock.Verify(x => x.GetByIdAsync(
            CategoryId,
            It.IsAny<Expression<Func<IQueryable<Category>, IQueryable<Category>>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
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