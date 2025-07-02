using ECommerce.Application.Features.Carts;

namespace ECommerce.Application.UnitTests.Features.Carts.V1.Commands;

public class AddToCartCommandHandlerTests : CartCommandsTestBase
{
    private readonly AddToCartCommandHandler _handler;
    private readonly AddToCartCommandValidator _validator;

    public AddToCartCommandHandlerTests()
    {
        _handler = new AddToCartCommandHandler(
            CartRepositoryMock.Object,
            ProductRepositoryMock.Object,
            CurrentUserServiceMock.Object,
            LazyServiceProviderMock.Object
        );
        _validator = new AddToCartCommandValidator(Localizer);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserIdIsMissing()
    {
        // Arrange
        CurrentUserServiceMock.Setup(s => s.UserId).Returns((string?)null);
        var command = new AddToCartCommand(Guid.NewGuid(), 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        SetupProductRepositoryGet(null);
        var command = new AddToCartCommand(Guid.NewGuid(), 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenProductIsNotActive()
    {
        // Arrange
        var product = CreateTestProduct(isActive: false);
        SetupProductRepositoryGet(product);
        var command = new AddToCartCommand(product.Id, 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.First().Should().Be(CartConsts.ErrorMessages.ProductNotActive);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnError_WhenProductStockIsInsufficient()
    {
        // Arrange
        var product = CreateTestProduct(stock: 5);
        SetupProductRepositoryGet(product);
        var command = new AddToCartCommand(product.Id, 10);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.First().Should().Be(CartConsts.ErrorMessages.InsufficientStock);
    }

    [Fact]
    public async Task Handle_ShouldCreateCartAndAddItem_WhenCartDoesNotExist()
    {
        // Arrange
        var product = CreateTestProduct(productId: DefaultProductId);
        SetupProductRepositoryGet(product);
        SetupCartRepositoryGet(null);
        var command = new AddToCartCommand(product.Id, 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        CartRepositoryMock.Verify(r => r.AddAsync(It.Is<Cart>(c => c.UserId == DefaultUserId), It.IsAny<CancellationToken>()), Times.Once);
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalItems.Should().Be(1);
        result.Value.TotalAmount.Should().Be(product.Price.Value);
    }

    [Fact]
    public async Task Handle_ShouldAddItemToExistingCart_WhenCartExists()
    {
        // Arrange
        var product = CreateTestProduct(productId: DefaultProductId);
        var cart = Cart.Create(DefaultUserId);
        SetupProductRepositoryGet(product);
        SetupCartRepositoryGet(cart);
        var command = new AddToCartCommand(product.Id, 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        cart.Items.Should().HaveCount(1);
        cart.Items.First().ProductId.Should().Be(product.Id);
    }

    [Theory]
    [InlineData(0, CartConsts.ValidationMessages.QuantityMustBePositive)]
    public async Task Validate_WithInvalidQuantity_ShouldReturnError(int quantity, string expectedError)
    {
        // Arrange
        var command = new AddToCartCommand(Guid.NewGuid(), quantity);

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(expectedError);
    }
} 