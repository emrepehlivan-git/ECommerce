
namespace ECommerce.Application.UnitTests.Features.Carts.V1.Commands;

public class UpdateCartItemQuantityCommandHandlerTests : CartCommandsTestBase
{
    private readonly UpdateCartItemQuantityCommandHandler _handler;
    private readonly UpdateCartItemQuantityCommandValidator _validator;

    public UpdateCartItemQuantityCommandHandlerTests()
    {
        _handler = new UpdateCartItemQuantityCommandHandler(
            CartRepositoryMock.Object,
            ProductRepositoryMock.Object,
            CurrentUserServiceMock.Object,
            LazyServiceProviderMock.Object
        );
        _validator = new UpdateCartItemQuantityCommandValidator(LocalizerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserIdIsMissing()
    {
        // Arrange
        CurrentUserServiceMock.Setup(s => s.UserId).Returns((string?)null);
        var command = new UpdateCartItemQuantityCommand(Guid.NewGuid(), 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Unauthorized);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCartDoesNotExist()
    {
        // Arrange
        SetupCartRepositoryGet(null);
        var command = new UpdateCartItemQuantityCommand(Guid.NewGuid(), 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.First().Should().Be(LocalizerMock.Object[CartConsts.ErrorMessages.CartNotFound]);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCartItemDoesNotExist()
    {
        // Arrange
        var cart = Cart.Create(DefaultUserId);
        SetupCartRepositoryGet(cart);
        var command = new UpdateCartItemQuantityCommand(Guid.NewGuid(), 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.First().Should().Be(LocalizerMock.Object[CartConsts.ErrorMessages.CartItemNotFound]);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var product = CreateTestProduct(productId: DefaultProductId);
        var cart = Cart.Create(DefaultUserId);
        cart.AddItem(product.Id, product.Price.Value, 1);
        SetupCartRepositoryGet(cart);
        SetupProductRepositoryGet(null);
        var command = new UpdateCartItemQuantityCommand(product.Id, 5);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.NotFound);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnError_WhenStockIsInsufficient()
    {
        // Arrange
        var product = CreateTestProduct(stock: 3, productId: DefaultProductId);
        var cart = Cart.Create(DefaultUserId);
        cart.AddItem(product.Id, product.Price.Value, 1);
        SetupCartRepositoryGet(cart);
        SetupProductRepositoryGet(product);
        var command = new UpdateCartItemQuantityCommand(product.Id, 5);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.First().Should().Be(LocalizerMock.Object[CartConsts.ErrorMessages.InsufficientStock]);
    }
    
    [Fact]
    public async Task Handle_ShouldUpdateQuantityAndReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var product = CreateTestProduct(stock: 10, productId: DefaultProductId);
        var cart = Cart.Create(DefaultUserId);
        cart.AddItem(product.Id, product.Price.Value, 1);
        SetupCartRepositoryGet(cart);
        SetupProductRepositoryGet(product);
        var command = new UpdateCartItemQuantityCommand(product.Id, 5);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        cart.Items.First().Quantity.Should().Be(5);
        result.Value.TotalItems.Should().Be(1);
    }

    [Theory]
    [InlineData(0, CartConsts.ValidationMessages.QuantityMustBePositive)]
    public async Task Validate_ShouldReturnError_WhenQuantityIsInvalid(int quantity, string expectedError)
    {
        // Arrange
        var command = new UpdateCartItemQuantityCommand(Guid.NewGuid(), quantity);

        // Act
        var validationResult = await _validator.ValidateAsync(command);
        
        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(LocalizerMock.Object[expectedError]);
    }
} 