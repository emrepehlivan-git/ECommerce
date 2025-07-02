namespace ECommerce.Application.UnitTests.Features.Stock.V1.Queries;

public sealed class GetProductStockInfoTests : StockTestBase
{
    private readonly GetProductStockInfoHandler Handler;
    private readonly GetProductStockInfo Query;

    public GetProductStockInfoTests()
    {
        Query = new GetProductStockInfo(DefaultProduct.Id);

        Handler = new GetProductStockInfoHandler(
            ProductRepositoryMock.Object,
            LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidProduct_ShouldReturnStockQuantity()
    {
        // Arrange
        SetupProductRepositoryGetById(DefaultProduct);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(DefaultStock.Quantity);
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ShouldReturnNotFound()
    {
        // Arrange
        SetupProductRepositoryGetById(null);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}