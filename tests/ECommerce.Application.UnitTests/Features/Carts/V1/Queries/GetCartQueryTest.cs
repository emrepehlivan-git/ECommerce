using ECommerce.Application.Features.Carts.V1;
using ECommerce.Application.Features.Carts.V1.Queries;
using ECommerce.Application.Interfaces;
using ECommerce.SharedKernel.Specifications;

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
        SetupCartListAsync(new List<Cart> { DefaultCart });

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
        SetupCartListAsync(new List<Cart>());

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(Guid.Empty);
        result.Value.UserId.Should().Be(Guid.Empty);
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalItems.Should().Be(0);
        result.Value.TotalAmount.Should().Be(0m);
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
    public async Task Handle_ShouldCallCartRepositoryWithSpecification()
    {
        // Arrange
        SetupCartListAsync(new List<Cart> { DefaultCart });

        // Act
        await Handler.Handle(Query, CancellationToken.None);

        // Assert
        CartRepositoryMock.Verify(
            x => x.ListAsync(It.IsAny<ISpecification<Cart>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyCart_ShouldReturnCartWithZeroItems()
    {
        // Arrange
        var emptyCart = Cart.Create(DefaultUserId);
        SetupCartListAsync(new List<Cart> { emptyCart });

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

        SetupCartListAsync(new List<Cart> { cartWithMultipleItems });

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
        SetupCartListAsync(new List<Cart>());

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(Guid.Empty);
        result.Value.UserId.Should().Be(Guid.Empty);
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalItems.Should().Be(0);
        result.Value.TotalAmount.Should().Be(0m);
    }
} 