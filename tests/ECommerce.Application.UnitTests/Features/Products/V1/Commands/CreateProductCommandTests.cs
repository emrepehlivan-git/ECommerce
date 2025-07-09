
namespace ECommerce.Application.UnitTests.Features.Products.V1.Commands;
 
public sealed class CreateProductCommandTests : ProductCommandsTestBase
{
    private readonly CreateProductCommandHandler Handler;
    private readonly CreateProductCommand Command;
    private readonly CreateProductCommandValidator Validator;
    private readonly Mock<IStockRepository> StockRepositoryMock = new();
    public CreateProductCommandTests()
    {
        SetupDefaultLocalizationMessages();
        Command = new CreateProductCommand(
            Name: "Test Product",
            Description: "Test Description",
            Price: 100m,
            CategoryId: Guid.NewGuid(),
            StockQuantity: 10);

        Handler = new CreateProductCommandHandler(
            ProductRepositoryMock.Object,
            StockRepositoryMock.Object,
            CacheManagerMock.Object,
            LazyServiceProviderMock.Object);

        Validator = new CreateProductCommandValidator(
            CategoryRepositoryMock.Object,
            ProductRepositoryMock.Object,
            Localizer);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateProduct()
    {
        SetupProductRepositoryAdd(DefaultProduct);

        var result = await Handler.Handle(Command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(DefaultProduct.Id);
    }

    [Theory]
    [InlineData("", "Product name must be at least 3 characters long")]
    [InlineData("AB", "Product name must be at least 3 characters long")]
    public async Task Validate_WithInvalidName_ShouldReturnValidationError(string name, string expectedError)
    {
        var command = Command with { Name = name };

        SetupCategoryExists(true);

        var validationResult = await Validator.ValidateAsync(command);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(expectedError);

    }

    [Theory]
    [InlineData(0, "Product price must be greater than zero")]
    public async Task Validate_WithInvalidPrice_ShouldReturnValidationError(decimal price, string expectedError)
    {
        var command = Command with { Price = price };

        var validationResult = await Validator.ValidateAsync(command);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == expectedError);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "Product category not found")]
    public async Task Validate_WithNonExistentCategory_ShouldReturnValidationError(string categoryId, string expectedError)
    {
        var command = Command with { CategoryId = Guid.Parse(categoryId) };

        SetupCategoryExists(false);

        var validationResult = await Validator.ValidateAsync(command);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == expectedError);
    }
}