using ECommerce.Application.Features.Carts;
using ECommerce.Application.Features.Carts.Queries;
using ECommerce.Application.Helpers;

namespace ECommerce.Application.UnitTests.Features.Carts.Queries;

public sealed class GetCartQueryTest : CartQueriesTestsBase
{
    private readonly GetCartQueryHandler Handler;
    private readonly GetCartQuery Query;

    public GetCartQueryTest()
    {
        Query = new GetCartQuery();

        Handler = new GetCartQueryHandler(
            CartRepositoryMock.Object,
            CurrentUserServiceMock.Object,
            LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidUser_ShouldReturnCart()
    {
        // Arrange
        DefaultCart.AddItem(DefaultProduct.Id, DefaultProduct.Price, 2);
        SetupCartExists(true);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(DefaultCart.Id);
        result.Value.UserId.Should().Be(DefaultUserId);
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalItems.Should().Be(1);
        result.Value.TotalAmount.Should().Be(200m);
    }

    [Fact]
    public async Task Handle_WithNonExistentCart_ShouldReturnNotFound()
    {
        // Arrange
        SetupCartExists(false);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(Localizer[CartConsts.ErrorMessages.CartNotFound]);
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ShouldReturnUnauthorized()
    {
        // Arrange
        CurrentUserServiceMock.Reset();
        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(string.Empty);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task Handle_WithNullUserId_ShouldReturnUnauthorized()
    {
        // Arrange
        CurrentUserServiceMock.Reset();
        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns((string)null!);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task Handle_WithInvalidUserIdFormat_ShouldReturnUnauthorized()
    {
        // Arrange
        CurrentUserServiceMock.Reset();
        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns("invalid-guid-format");

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task Handle_ShouldCallCartRepositoryWithCorrectUserId()
    {
        // Arrange
        SetupCartExists(true);

        // Act
        await Handler.Handle(Query, CancellationToken.None);

        // Assert
        CartRepositoryMock.Verify(
            x => x.GetByUserIdWithItemsAsync(DefaultUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyCart_ShouldReturnCartWithZeroItems()
    {
        // Arrange
        var emptyCart = Cart.Create(DefaultUserId);
        CartRepositoryMock
            .Setup(x => x.GetByUserIdWithItemsAsync(DefaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyCart);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalItems.Should().Be(0);
        result.Value.TotalAmount.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_WithMultipleItems_ShouldReturnCorrectTotals()
    {
        // Arrange
        var cartWithMultipleItems = Cart.Create(DefaultUserId);
        cartWithMultipleItems.AddItem(DefaultProduct.Id, DefaultProduct.Price, 2);
        cartWithMultipleItems.AddItem(Guid.NewGuid(), 50m, 3);

        CartRepositoryMock
            .Setup(x => x.GetByUserIdWithItemsAsync(DefaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartWithMultipleItems);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalItems.Should().Be(2);
        result.Value.TotalAmount.Should().Be(350m);
    }

    [Fact]
    public async Task Handle_ShouldVerifyLocalizationServiceIsConfigured()
    {
        // Arrange
        SetupCartExists(false);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        LazyServiceProviderMock.Verify(
            x => x.LazyGetRequiredService<LocalizationHelper>(),
            Times.Once);
    }
} 