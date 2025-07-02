using ECommerce.Application.Features.Orders.V1.Commands;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.UnitTests.Features.Orders.Commands;

public sealed class OrderStatusUpdateCommandTests : OrderCommandsTestBase
{
    private readonly OrderStatusUpdateCommandHandler Handler;
    private readonly OrderStatusUpdateCommand Command;

    public OrderStatusUpdateCommandTests()
    {
        Command = new OrderStatusUpdateCommand(DefaultOrder.Id, OrderStatus.Processing);
        Handler = new OrderStatusUpdateCommandHandler(
            OrderRepositoryMock.Object,
            LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidTransition_ShouldUpdateStatus()
    {
        SetupOrderRepositoryGetByIdAsync(DefaultOrder);

        var result = await Handler.Handle(Command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        DefaultOrder.Status.Should().Be(OrderStatus.Processing);
        OrderRepositoryMock.Verify(x => x.Update(It.Is<Order>(o => o.Status == OrderStatus.Processing)), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidTransition_ShouldReturnError()
    {
        SetupOrderRepositoryGetByIdAsync(DefaultOrder);
        var invalidCommand = new OrderStatusUpdateCommand(DefaultOrder.Id, OrderStatus.Delivered);

        var result = await Handler.Handle(invalidCommand, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Should().Be("Invalid status transition");
        OrderRepositoryMock.Verify(x => x.Update(It.IsAny<Order>()), Times.Never);
    }
}
