namespace ECommerce.Application.UnitTests.Features.Products.V1.Commands;

public sealed class DeleteProductCommandTests : ProductCommandsTestBase
{
    private readonly DeleteProductCommandHandler Handler;
    private DeleteProductCommand Command;

    public DeleteProductCommandTests()
    {
        Command = new DeleteProductCommand(DefaultProduct.Id);

        Handler = new DeleteProductCommandHandler(
            ProductRepositoryMock.Object,
            CacheManagerMock.Object,
            LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        ProductRepositoryMock.Setup(r => r.GetByIdAsync(
            It.IsAny<Guid>(),
            It.IsAny<Expression<Func<IQueryable<Product>, IQueryable<Product>>>>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(DefaultProduct);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        ProductRepositoryMock.Verify(r => r.Delete(It.Is<Product>(p => p.Id == Command.Id)), Times.Once);
        CacheManagerMock.Verify(c => c.RemoveByPatternAsync("products:*", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "Product not found.")]
    public async Task Handle_WithNonExistingProduct_ShouldReturnNotFound(string productId, string expectedError)
    {
        Command = Command with { Id = Guid.Parse(productId) };

        SetupProductExists(false);

        var result = await Handler.Handle(Command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(expectedError);
    }
}