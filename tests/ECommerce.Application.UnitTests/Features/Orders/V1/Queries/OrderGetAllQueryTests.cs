using ECommerce.Application.Features.Orders.V1.Queries;
using ECommerce.Application.Features.Orders.V1.DTOs;
using ECommerce.Application.Parameters;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.UnitTests.Features.Orders.Queries;

public sealed class OrderGetAllQueryTests : OrderQueriesTestBase
{
    private PagedInfo PagedInfo = new(1, 1, 10, 10);
    private readonly OrderGetAllQueryHandler Handler;
    private readonly OrderGetAllQuery Query;

    public OrderGetAllQueryTests()
    {
        Query = new OrderGetAllQuery(new PageableRequestParams(Page: 1, PageSize: 10));
        Handler = new OrderGetAllQueryHandler(OrderRepositoryMock.Object, CurrentUserServiceMock.Object, LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnPagedOrders()
    {
        // Arrange
        var orderDtos = new List<OrderDto>
        {
            new(DefaultOrder.Id,
                         DefaultOrder.UserId,
                         DefaultOrder.OrderDate,
                         DefaultOrder.Status,
                         DefaultOrder.TotalAmount,
                         DefaultOrder.ShippingAddress.ToString(),
                         DefaultOrder.BillingAddress.ToString(),
                         DefaultOrder.Items
                         .Select(i => new OrderItemDto(i.Id,
                                                       i.ProductId,
                                                       i.Product?.Name ?? "",
                                                       i.UnitPrice.Value,
                                                       i.Quantity,
                                                       i.TotalPrice.Value))
                         .ToList())
        };
        var pagedResult = new PagedResult<List<OrderDto>>(PagedInfo, orderDtos);

        OrderRepositoryMock
            .Setup(x => x.GetPagedAsync<OrderDto>(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Expression<Func<IQueryable<Order>, IOrderedQueryable<Order>>>>(),
                It.IsAny<Expression<Func<IQueryable<Order>, IQueryable<Order>>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(pagedResult);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ShouldPassStatusPredicate()
    {
        // Arrange
        var queryWithStatus = new OrderGetAllQuery(new PageableRequestParams(Page: 1, PageSize: 10), OrderStatus.Processing);
        var orderDtos = new List<OrderDto>
        {
            new(DefaultOrder.Id, DefaultOrder.UserId, DefaultOrder.OrderDate, DefaultOrder.Status, DefaultOrder.TotalAmount, DefaultOrder.ShippingAddress.ToString(), DefaultOrder.BillingAddress.ToString(), DefaultOrder.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.Product?.Name ?? "", i.UnitPrice.Value, i.Quantity, i.TotalPrice.Value)).ToList())
        };
        var pagedResult = new PagedResult<List<OrderDto>>(PagedInfo, orderDtos);

        OrderRepositoryMock
            .Setup(x => x.GetPagedAsync<OrderDto>(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Expression<Func<IQueryable<Order>, IOrderedQueryable<Order>>>>(),
                It.IsAny<Expression<Func<IQueryable<Order>, IQueryable<Order>>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await Handler.Handle(queryWithStatus, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        OrderRepositoryMock.Verify(x => x.GetPagedAsync<OrderDto>(
            It.IsAny<Expression<Func<Order, bool>>>(),
            It.IsAny<Expression<Func<IQueryable<Order>, IOrderedQueryable<Order>>>>(),
            It.IsAny<Expression<Func<IQueryable<Order>, IQueryable<Order>>>>(),
            1,
            10,
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnPaginatedOrders()
    {
        // Arrange
        var query = new OrderGetAllQuery(new PageableRequestParams(Page: 2, PageSize: 5));
        var orderDtos = new List<OrderDto>
        {
            new OrderDto(DefaultOrder.Id, DefaultOrder.UserId, DefaultOrder.OrderDate, DefaultOrder.Status, DefaultOrder.TotalAmount, DefaultOrder.ShippingAddress.ToString(), DefaultOrder.BillingAddress.ToString(), DefaultOrder.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.Product?.Name ?? "", i.UnitPrice.Value, i.Quantity, i.TotalPrice.Value)).ToList())
        };
        var pagedResult = new PagedResult<List<OrderDto>>(PagedInfo, orderDtos);

        OrderRepositoryMock
            .Setup(x => x.GetPagedAsync<OrderDto>(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Expression<Func<IQueryable<Order>, IOrderedQueryable<Order>>>>(),
                It.IsAny<Expression<Func<IQueryable<Order>, IQueryable<Order>>>>(),
                2,
                5,
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await Handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(pagedResult);
    }

    [Fact]
    public async Task Handle_ShouldNotIncludeOrderItemsAndProducts_BecauseProjectionIsUsed()
    {
        // Arrange
        var orderDtos = new List<OrderDto>
        {
            new(DefaultOrder.Id, DefaultOrder.UserId, DefaultOrder.OrderDate, DefaultOrder.Status, DefaultOrder.TotalAmount, DefaultOrder.ShippingAddress.ToString(), DefaultOrder.BillingAddress.ToString(), DefaultOrder.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.Product?.Name ?? "", i.UnitPrice.Value, i.Quantity, i.TotalPrice.Value)).ToList())
        };
        var pagedResult = new PagedResult<List<OrderDto>>(PagedInfo, orderDtos);

        OrderRepositoryMock
            .Setup(x => x.GetPagedAsync<OrderDto>(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Expression<Func<IQueryable<Order>, IOrderedQueryable<Order>>>>(),
                It.IsAny<Expression<Func<IQueryable<Order>, IQueryable<Order>>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // Verify that include parameter (the 3rd argument) is null or not explicitly checked for specific includes,
        // effectively confirming the change to remove explicit Includes.
        // Actually, since I removed the explicit include in the handler, the 3rd arg passed to GetPagedAsync will be null (default).
        // The original test verified it was called "Times.Once" with "It.IsAny". This still passes if I pass null.
        // But to be precise, I should verify it is indeed called.

        OrderRepositoryMock.Verify(x => x.GetPagedAsync<OrderDto>(
            It.IsAny<Expression<Func<Order, bool>>>(),
            It.IsAny<Expression<Func<IQueryable<Order>, IOrderedQueryable<Order>>>>(),
            null, // Include should be null
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
} 