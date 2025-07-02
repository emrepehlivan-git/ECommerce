namespace ECommerce.Application.UnitTests.Features.Stock.V1.Commands;

public sealed class UpdateProductStockTests : StockTestBase
{
    private readonly UpdateProductStockHandler Handler;
    private readonly UpdateProductStockValidator Validator;
    private readonly UpdateProductStock Command;

    public UpdateProductStockTests()
    {
        Command = new UpdateProductStock(DefaultProduct.Id, 5);

        Handler = new UpdateProductStockHandler(
            ProductRepositoryMock.Object,
            StockRepositoryMock.Object,
            LazyServiceProviderMock.Object);


        Validator = new UpdateProductStockValidator(
            ProductRepositoryMock.Object,
            Localizer);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReserveStock()
    {
        // Arrange
        SetupProductRepositoryGetById(DefaultProduct);
        SetupStockRepositoryReserveStock();

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        StockRepositoryMock.Verify(
            x => x.ReserveStockAsync(
                Command.ProductId,
                Command.StockQuantity,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ShouldReturnNotFound()
    {
        // Arrange
        SetupProductRepositoryGetById(null);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_WithInvalidStockQuantity_ShouldReturnValidationError(int stockQuantity)
    {
        // Arrange
        var command = Command with { StockQuantity = stockQuantity };
        SetupProductExists(true);

        // Act
        var validationResult = await Validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Stock quantity must be greater than zero.");
    }

    [Fact]
    public async Task Validate_WithNonExistentProduct_ShouldReturnValidationError()
    {
        // Arrange
        SetupProductRepositoryGetById(null);

        // Act
        var validationResult = await Validator.ValidateAsync(Command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Product not found.");
    }
}