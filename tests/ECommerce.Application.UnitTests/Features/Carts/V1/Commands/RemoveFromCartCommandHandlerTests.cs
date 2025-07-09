using ECommerce.Application.Features.Carts;

namespace ECommerce.Application.UnitTests.Features.Carts.V1.Commands;

public class RemoveFromCartCommandHandlerTests : CartCommandsTestBase
{
    private readonly RemoveFromCartCommandHandler _handler;
    private readonly RemoveFromCartCommandValidator _validator;

    public RemoveFromCartCommandHandlerTests()
    {
        _handler = new RemoveFromCartCommandHandler(
            CartRepositoryMock.Object,
            CurrentUserServiceMock.Object,
            LazyServiceProviderMock.Object
        );
        _validator = new RemoveFromCartCommandValidator(LocalizerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserIdIsMissing()
    {
        // Arrange
        CurrentUserServiceMock.Setup(s => s.UserId).Returns((string?)null);
        var command = new RemoveFromCartCommand(Guid.NewGuid());

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
        var command = new RemoveFromCartCommand(Guid.NewGuid());

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
        var command = new RemoveFromCartCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.First().Should().Be(LocalizerMock.Object[CartConsts.ErrorMessages.CartItemNotFound]);
    }
    
    [Fact]
    public async Task Handle_ShouldRemoveItemAndReturnSuccess_WhenItemExists()
    {
        // Arrange
        var product = CreateTestProduct(productId: DefaultProductId);
        var cart = Cart.Create(DefaultUserId);
        cart.AddItem(product.Id, product.Price.Value, 1);
        
        SetupCartRepositoryGet(cart);
        var command = new RemoveFromCartCommand(product.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        cart.Items.Should().BeEmpty();
        result.Value.TotalItems.Should().Be(0);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenProductIdIsEmpty()
    {
        // Arrange
        var command = new RemoveFromCartCommand(Guid.Empty);

        // Act
        var validationResult = await _validator.ValidateAsync(command);
        
        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(LocalizerMock.Object[CartConsts.ValidationMessages.ProductIdRequired]);
    }
} 