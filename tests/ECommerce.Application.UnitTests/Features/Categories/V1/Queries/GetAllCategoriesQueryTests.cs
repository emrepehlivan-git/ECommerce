using ECommerce.Application.Behaviors;
using ECommerce.Application.Parameters;
using Mapster;

namespace ECommerce.Application.UnitTests.Features.Categories.V1.Queries;

public sealed class GetAllCategoriesQueryTests : CategoryQueriesTestBase
{
    private readonly GetAllCategoriesQueryHandler Handler;
    private readonly GetAllCategoriesQuery Query;
    private readonly List<CategoryDto> Categories;
    private readonly PagedInfo PagedInfo = new(1, 3, 10, 1);

    public GetAllCategoriesQueryTests()
    {
        Categories =
        [
            new(Guid.NewGuid(), "Category 1"),
            new(Guid.NewGuid(), "Category 2"),
            new(Guid.NewGuid(), "Category 3")
        ];

        Query = new GetAllCategoriesQuery(new PageableRequestParams(Page: 1, PageSize: 10));
        Handler = new GetAllCategoriesQueryHandler(CategoryRepositoryMock.Object, LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnCategories()
    {
        // Arrange
        var pagedResult = new PagedResult<List<CategoryDto>>(PagedInfo, Categories);
        CategoryRepositoryMock
            .Setup(x => x.GetPagedAsync<CategoryDto>(
                It.IsAny<Expression<Func<Category, bool>>>(),
                It.IsAny<Expression<Func<IQueryable<Category>, IOrderedQueryable<Category>>>>(),
                It.IsAny<Expression<Func<IQueryable<Category>, IQueryable<Category>>>>(),
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
    public async Task Handle_WithPagination_ShouldReturnPaginatedCategories()
    {
        // Arrange
        var query = new GetAllCategoriesQuery(new PageableRequestParams(Page: 1, PageSize: 2));
        var paginatedCategories = Categories.Take(2).ToList();
        var pagedResult = new PagedResult<List<CategoryDto>>(new PagedInfo(1, 3, 2, 2), paginatedCategories);
        
        CategoryRepositoryMock
            .Setup(x => x.GetPagedAsync<CategoryDto>(
                It.IsAny<Expression<Func<Category, bool>>>(),
                It.IsAny<Expression<Func<IQueryable<Category>, IOrderedQueryable<Category>>>>(),
                It.IsAny<Expression<Func<IQueryable<Category>, IQueryable<Category>>>>(),
                1,
                2,
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await Handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(pagedResult);
    }

    [Fact]
    public async Task Handle_WithOrderBy_ShouldPassOrderByToRepository()
    {
        // Arrange
        var query = new GetAllCategoriesQuery(new PageableRequestParams(Page: 1, PageSize: 10), OrderBy: "Name desc");
        var pagedResult = new PagedResult<List<CategoryDto>>(PagedInfo, Categories);
        
        CategoryRepositoryMock
            .Setup(x => x.GetPagedAsync<CategoryDto>(
                It.IsAny<Expression<Func<Category, bool>>>(),
                It.IsAny<Expression<Func<IQueryable<Category>, IOrderedQueryable<Category>>>>(),
                It.IsAny<Expression<Func<IQueryable<Category>, IQueryable<Category>>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await Handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        CategoryRepositoryMock.Verify(x => x.GetPagedAsync<CategoryDto>(
            It.IsAny<Expression<Func<Category, bool>>>(),
            It.IsAny<Expression<Func<IQueryable<Category>, IOrderedQueryable<Category>>>>(),
            It.IsAny<Expression<Func<IQueryable<Category>, IQueryable<Category>>>>(),
            1,
            10,
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Query_ShouldImplementICacheableRequest()
    {
        // Arrange
        var pageableParams = new PageableRequestParams(Page: 1, PageSize: 10);
        var orderBy = "Name asc";
        var query = new GetAllCategoriesQuery(pageableParams, orderBy);

        // Act & Assert
        query.Should().BeAssignableTo<ICacheableRequest>();
        query.CacheKey.Should().Be("categories:page-1:size-10:search-empty:order-Name asc");
        query.CacheDuration.Should().Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public void Query_WithNullOrderBy_ShouldHaveDefaultCacheKey()
    {
        // Arrange
        var pageableParams = new PageableRequestParams(Page: 2, PageSize: 5);
        var query = new GetAllCategoriesQuery(pageableParams, null);

        // Act & Assert
        query.CacheKey.Should().Be("categories:page-2:size-5:search-empty:order-default");
    }

    [Fact]
    public void Query_WithSearch_ShouldIncludeSearchInCacheKey()
    {
        // Arrange
        var pageableParams = new PageableRequestParams(Page: 1, PageSize: 10, Search: "electronics");
        var query = new GetAllCategoriesQuery(pageableParams, null);

        // Act & Assert
        query.CacheKey.Should().Contain("search-electronics");
    }
}
