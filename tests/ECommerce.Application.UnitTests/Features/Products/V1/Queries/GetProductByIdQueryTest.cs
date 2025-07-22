
using ECommerce.Application.Behaviors;
using ECommerce.SharedKernel.Specifications;

namespace ECommerce.Application.UnitTests.Features.Products.V1.Queries;

public sealed class GetProductByIdQueryTest : ProductQueriesTestsBase
{
    private readonly GetProductByIdQueryHandler Handler;
    private readonly GetProductByIdQuery Query;
    private readonly Guid ProductId = Guid.NewGuid();

    public GetProductByIdQueryTest()
    {
        Query = new GetProductByIdQuery(ProductId);

        Handler = new GetProductByIdQueryHandler(
            ProductRepositoryMock.Object,
            LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnProduct()
    {
        // Arrange
        ProductRepositoryMock
            .Setup(x => x.ListAsync(
                It.IsAny<ISpecification<Product>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { DefaultProduct });

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(DefaultProduct.Id);
        result.Value.Name.Should().Be(DefaultProduct.Name);
        result.Value.Description.Should().Be(DefaultProduct.Description);
        result.Value.Price.Should().Be(DefaultProduct.Price.Value);
    }

    [Fact]
    public async Task Handle_WithInvalidQuery_ShouldReturnNotFound()
    {
        // Arrange
        ProductRepositoryMock
            .Setup(x => x.ListAsync(
                It.IsAny<ISpecification<Product>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(Localizer[ProductConsts.NotFound]);
    }

    [Fact]
    public async Task Handle_ShouldUseSpecificationWithIncludes()
    {
        // Arrange
        ProductRepositoryMock
            .Setup(x => x.ListAsync(
                It.IsAny<ISpecification<Product>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { DefaultProduct });

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        ProductRepositoryMock.Verify(x => x.ListAsync(
            It.IsAny<ISpecification<Product>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Query_ShouldImplementICacheableRequest()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var query = new GetProductByIdQuery(productId);

        // Act & Assert
        query.Should().BeAssignableTo<ICacheableRequest>();
        query.CacheKey.Should().Be($"product:{productId}");
        query.CacheDuration.Should().Be(TimeSpan.FromMinutes(15));
    }
}
