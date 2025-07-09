namespace ECommerce.Application.UnitTests.Features.Products.V1.Commands;

public sealed class UpdateProductCommandTests : ProductCommandsTestBase
{
    private readonly UpdateProductCommandHandler Handler;
    private UpdateProductCommand Command;
    private readonly UpdateProductCommandValidator Validator;

    public UpdateProductCommandTests()
    {
        Command = new UpdateProductCommand(
            Id: DefaultProduct.Id,
            Name: "Updated Product",
            Description: "Updated Description",
            Price: 200m,
            CategoryId: Guid.NewGuid());

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

    private void SetupValidationMocks(bool productExists = true, bool categoryExists = true, bool productNameExists = false)
    {
        ProductRepositoryMock
            .Setup(r => r.AnyAsync(It.Is<Expression<Func<Product, bool>>>(expr => expr.Body.ToString().Contains("Id ==")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productExists);
        ProductRepositoryMock
            .Setup(r => r.AnyAsync(It.Is<Expression<Func<Product, bool>>>(expr => expr.Body.ToString().Contains("Name.ToLower()")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productNameExists);
        CategoryRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoryExists);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var product = DefaultProduct;
        ProductRepositoryMock.Setup(r => r.GetByIdAsync(
            It.IsAny<Guid>(),
            It.IsAny<Expression<Func<IQueryable<Product>, IQueryable<Product>>>>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(product);
        CacheManagerMock.Setup(c => c.RemoveByPatternAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        ProductRepositoryMock.Verify(r => r.Update(It.Is<Product>(p => p.Id == Command.Id)), Times.Once);
        CacheManagerMock.Verify(c => c.RemoveByPatternAsync("products:*", It.IsAny<CancellationToken>()), Times.Once);
    }

    /*
    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "Product not found.", "Product category not found")]
    public async Task Validate_WithNonExistentProduct_ShouldReturnValidationError(string productId, string expectedError1, string expectedError2)
    {
        Command = Command with { Id = Guid.Parse(productId) };

        SetupValidationMocks(productExists: false, categoryExists: false);

        var validationResult = await Validator.ValidateAsync(Command);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == expectedError1);
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == expectedError2);
    }
    */

    [Theory]
    [InlineData("", "Product name must be at least 3 characters long")]
    [InlineData("AB", "Product name must be at least 3 characters long")]
    public async Task Validate_WithInvalidName_ShouldReturnValidationError(string name, string expectedError)
    {
        Command = Command with { Name = name };
        
        SetupValidationMocks();

        var validationResult = await Validator.ValidateAsync(Command);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(expectedError);
    }

    /*
    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "Product with this name already exists")]
    public async Task Validate_WithDuplicateName_ShouldReturnValidationError(string productId, string expectedError)
    {
        Command = Command with { Id = Guid.Parse(productId) };

        SetupValidationMocks(productNameExists: true);

        var validationResult = await Validator.ValidateAsync(Command);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(expectedError);
    }

    [Theory]
    [InlineData(0, "Product price must be greater than zero")]
    public async Task Validate_WithInvalidPrice_ShouldReturnValidationError(decimal price, string expectedError)
    {
        Command = Command with { Price = price };
        
        SetupValidationMocks();
        
        var validationResult = await Validator.ValidateAsync(Command);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(expectedError);
    }

    [Theory]
    [InlineData("10000000-0000-0000-0000-000000000000", "Product category not found")]
    public async Task Validate_WithNonExistentCategory_ShouldReturnValidationError(string categoryId, string expectedError)
    {
        Command = Command with { CategoryId = Guid.Parse(categoryId) };
        
        SetupValidationMocks(categoryExists: false);
        
        var validationResult = await Validator.ValidateAsync(Command);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(expectedError);
    }
    */
}