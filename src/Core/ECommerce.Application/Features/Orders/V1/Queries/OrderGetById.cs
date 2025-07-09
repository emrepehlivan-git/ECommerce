using Ardalis.Result;
using ECommerce.Application.Common.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Features.Orders.V1.DTOs;
using ECommerce.Application.Repositories;
using ECommerce.SharedKernel;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Orders.V1.Queries;

public sealed record OrderGetByIdQuery(Guid Id) : IRequest<Result<OrderDto>>;

public sealed class OrderGetByIdQueryHandler(
    IOrderRepository orderRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<OrderGetByIdQuery, Result<OrderDto>>(lazyServiceProvider)
{
    public override async Task<Result<OrderDto>> Handle(OrderGetByIdQuery query, CancellationToken cancellationToken)
    {
        var order = await orderRepository.Query()
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);

        if (order is null)
            return Result.NotFound(Localizer[OrderConsts.NotFound]);

        return Result.Success(order.Adapt<OrderDto>());
    }
}