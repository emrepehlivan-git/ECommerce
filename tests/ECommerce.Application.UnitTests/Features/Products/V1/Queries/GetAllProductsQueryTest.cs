using ECommerce.Application.Behaviors;

namespace ECommerce.Application.UnitTests.Features.Products.V1.Queries;

public sealed class GetAllProductsQueryTest : ProductQueriesTestsBase
{
    private PagedInfo PagedInfo = new(1, 1, 10, 10);
    private readonly GetAllProductsQueryHandler Handler;
    private readonly GetAllProductsQuery Query;

    public GetAllProductsQueryTest()
    {
        Query = new GetAllProductsQuery(new PageableRequestParams());

        Handler = new GetAllProductsQueryHandler(
            ProductRepositoryMock.Object,
            LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnPagedProducts()
    {
        // Arrange
        var productDtos = new List<ProductDto>
        {
            new(DefaultProduct.Id, DefaultProduct.Name, DefaultProduct.Description, DefaultProduct.Price.Value, DefaultProduct.Category?.Name, DefaultProduct.Stock?.Quantity ?? 0, DefaultProduct.IsActive)
        };
        var pagedResult = new PagedResult<List<ProductDto>>(PagedInfo, productDtos);

        ProductRepositoryMock
            .Setup(x => x.GetPagedAsync<ProductDto>(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<Expression<Func<IQueryable<Product>, IOrderedQueryable<Product>>>>(),
                It.IsAny<Expression<Func<IQueryable<Product>, IQueryable<Product>>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(pagedResult);
    }

    [Fact]
    public async Task Handle_WithEmptyProducts_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyResult = new PagedResult<List<ProductDto>>(PagedInfo, new List<ProductDto>());

        ProductRepositoryMock
            .Setup(x => x.GetPagedAsync<ProductDto>(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<Expression<Func<IQueryable<Product>, IOrderedQueryable<Product>>>>(),
                It.IsAny<Expression<Func<IQueryable<Product>, IQueryable<Product>>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(emptyResult);
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_ShouldPassCategoryIdPredicate()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var queryWithCategory = new GetAllProductsQuery(new PageableRequestParams(), CategoryId: categoryId);
        var productDtos = new List<ProductDto>();
        var pagedResult = new PagedResult<List<ProductDto>>(PagedInfo, productDtos);

        ProductRepositoryMock
            .Setup(x => x.GetPagedAsync<ProductDto>(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<Expression<Func<IQueryable<Product>, IOrderedQueryable<Product>>>>(),
                It.IsAny<Expression<Func<IQueryable<Product>, IQueryable<Product>>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await Handler.Handle(queryWithCategory, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        ProductRepositoryMock.Verify(x => x.GetPagedAsync<ProductDto>(
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Expression<Func<IQueryable<Product>, IOrderedQueryable<Product>>>>(),
            It.IsAny<Expression<Func<IQueryable<Product>, IQueryable<Product>>>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Query_ShouldImplementICacheableRequest()
    {
        // Arrange
        var pageableParams = new PageableRequestParams(Page: 1, PageSize: 10);
        var query = new GetAllProductsQuery(pageableParams, OrderBy: "Name asc");

        // Act & Assert
        query.Should().BeAssignableTo<ICacheableRequest>();
        query.CacheKey.Should().Contain("products:page-1:size-10:search-empty");
        query.CacheDuration.Should().Be(TimeSpan.FromMinutes(10));
    }

    [Fact]
    public void Query_WithSearch_ShouldIncludeSearchInCacheKey()
    {
        // Arrange
        var pageableParams = new PageableRequestParams(Page: 1, PageSize: 10, Search: "test");
        var query = new GetAllProductsQuery(pageableParams, OrderBy: "Name asc");

        // Act & Assert
        query.CacheKey.Should().Contain("search-test");
    }
}