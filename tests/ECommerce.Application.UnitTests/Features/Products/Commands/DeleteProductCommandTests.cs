namespace ECommerce.Application.UnitTests.Features.Products.Commands;

public sealed class DeleteProductCommandTests : ProductCommandsTestBase
{
    private readonly DeleteProductCommandHandler Handler;
    private DeleteProductCommand Command;

    public DeleteProductCommandTests()
    {
        Command = new DeleteProductCommand(Id: Guid.NewGuid());
        Handler = new DeleteProductCommandHandler(
            ProductRepositoryMock.Object,
            CacheManagerMock.Object,
            LazyServiceProviderMock.Object);
        SetupDefaultLocalizationMessages();
    }

    [Fact]
    public async Task Handle_WithExistingProduct_ShouldDeleteProduct()
    {
        // Arrange
        SetupProductRepositoryGetByIdAsync(DefaultProduct);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
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