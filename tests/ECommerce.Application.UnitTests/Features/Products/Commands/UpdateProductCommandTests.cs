using ECommerce.Application.Features.Products;

namespace ECommerce.Application.UnitTests.Features.Products.Commands;

public sealed class UpdateProductCommandTests : ProductCommandsTestBase
{
    private readonly UpdateProductCommandHandler Handler;
    private readonly UpdateProductCommandValidator Validator;
    private UpdateProductCommand Command;

    public UpdateProductCommandTests()
    {
        Command = new UpdateProductCommand(
            Id: DefaultProduct.Id,
            Name: "Updated Product",
            Description: "Updated Description",
            Price: 150m,
            CategoryId: DefaultProduct.CategoryId);

        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();


        Handler = new UpdateProductCommandHandler(
            ProductRepositoryMock.Object,
            CacheManagerMock.Object,
            LazyServiceProviderMock.Object);

        Validator = new UpdateProductCommandValidator(
            ProductRepositoryMock.Object,
            CategoryRepositoryMock.Object,
            Localizer);
        SetupDefaultLocalizationMessages();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateProduct()
    {
        // Arrange
        SetupProductExists(true);
        SetupCategoryExists(true);
        SetupProductRepositoryGetByIdAsync(DefaultProduct);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        // Verify that the repository's Update method was called with the updated product
        ProductRepositoryMock.Verify(
            x => x.Update(It.Is<Product>(p =>
                p.Name == Command.Name &&
                p.Description == Command.Description &&
                p.Price.Value == Command.Price &&
                p.CategoryId == Command.CategoryId)),
            Times.Once);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "Product not found.", "Product category not found")]
    public async Task Validate_WithNonExistentProduct_ShouldReturnValidationError(string productId, string expectedError1, string expectedError2)
    {
        Command = Command with { Id = Guid.Parse(productId) };

        SetupProductExists(false);
        SetupCategoryExists(false);

        var validationResult = await Validator.ValidateAsync(Command);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(2);
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == expectedError1);
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == expectedError2);
    }

    [Theory]
    [InlineData("", "Product name must be at least 3 characters long")]
    [InlineData("AB", "Product name must be at least 3 characters long")]
    public async Task Validate_WithInvalidName_ShouldReturnValidationError(string name, string expectedError)
    {
        var command = Command with { Name = name };

        SetupProductExists(true);
        SetupCategoryExists(true);

        var validationResult = await Validator.ValidateAsync(command);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == expectedError);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "Product with this name already exists")]
    public async Task Validate_WithDuplicateName_ShouldReturnValidationError(string productId, string expectedError)
    {
        Command = Command with { Id = Guid.Parse(productId) };

        SetupProductExists(true);
        SetupCategoryExists(true);

        var validationResult = await Validator.ValidateAsync(Command);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == expectedError);
    }

    [Fact]
    public async Task Validate_WithInvalidPrice_ShouldReturnValidationError()
    {
        var command = Command with { Price = 0m };
        var expectedError = "Product price must be greater than zero";

        SetupProductExists(true);
        SetupCategoryExists(true);

        var validationResult = await Validator.ValidateAsync(command);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == expectedError);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "Product category not found")]
    public async Task Validate_WithNonExistentCategory_ShouldReturnValidationError(string productId, string expectedError)
    {
        Command = Command with { Id = Guid.Parse(productId) };

        SetupProductExists(true);
        SetupCategoryExists(false);

        var validationResult = await Validator.ValidateAsync(Command);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == expectedError);
    }
}