using Ardalis.Result;
using ECommerce.Application.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Features.Orders.V1.DTOs;
using ECommerce.Application.Repositories;
using ECommerce.SharedKernel;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Orders.V1.Queries;

public sealed record OrderGetByUserQuery(Guid UserId) : IRequest<Result<List<OrderDto>>>;

public sealed class OrderGetByUserQueryHandler(
    IOrderRepository orderRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<OrderGetByUserQuery, Result<List<OrderDto>>>(lazyServiceProvider)
{
    public override async Task<Result<List<OrderDto>>> Handle(OrderGetByUserQuery query, CancellationToken cancellationToken)
    {
        return await orderRepository.Query(
            x => x.UserId == query.UserId,
            x => x.Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .OrderByDescending(x => x.OrderDate)
        )
        .ProjectToType<OrderDto>()
        .ToListAsync(cancellationToken);
    }
}