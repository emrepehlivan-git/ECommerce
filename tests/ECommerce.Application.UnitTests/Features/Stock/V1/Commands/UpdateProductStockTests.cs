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
            LazyServiceProviderMock.Object);


        Validator = new UpdateProductStockValidator(
            LocalizerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateStock()
    {
        // Arrange
        SetupProductRepositoryGetById(DefaultProduct);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        DefaultProduct.Stock.Quantity.Should().Be(Command.StockQuantity);
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
    [InlineData(-1)]
    public async Task Validate_WithInvalidStockQuantity_ShouldReturnValidationError(int stockQuantity)
    {
        // Arrange
        var command = Command with { StockQuantity = stockQuantity };

        // Act
        var validationResult = await Validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(LocalizerMock.Object[ProductConsts.StockQuantityMustBeGreaterThanZero]);
    }
}