using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Helpers;
using ECommerce.Application.Repositories;
using ECommerce.Domain.Enums;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Orders.Commands;

public sealed record OrderCancelCommand(Guid OrderId) : IRequest<Result>, IValidatableRequest, ITransactionalRequest;

public sealed class OrderCancelCommandValidator : AbstractValidator<OrderCancelCommand>
{
    public OrderCancelCommandValidator(
        IOrderRepository orderRepository,
        LocalizationHelper localizer)
    {
        RuleFor(x => x.OrderId)
            .MustAsync(async (id, ct) =>
                await orderRepository.AnyAsync(x => x.Id == id, cancellationToken: ct))
            .WithMessage(localizer[OrderConsts.NotFound]);

        RuleFor(x => x.OrderId)
            .MustAsync(async (id, ct) =>
            {
                var order = await orderRepository.Query(x => x.Id == id)
                    .FirstOrDefaultAsync(cancellationToken: ct);

                return order is not null && order.Status != OrderStatus.Shipped && order.Status != OrderStatus.Delivered;
            })
            .WithMessage(localizer[OrderConsts.OrderCannotBeCancelled]);
    }
}

public sealed class OrderCancelCommandHandler(
    IOrderRepository orderRepository,
    IStockRepository stockRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<OrderCancelCommand, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(OrderCancelCommand command, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(command.OrderId,
            include: query => query.Include(o => o.Items),
            cancellationToken: cancellationToken);

        if (order is null || order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
            return Result.Error(Localizer[OrderConsts.OrderCannotBeCancelled]);

        foreach (var item in order.Items)
        {
            await stockRepository.ReleaseStockAsync(item.ProductId, item.Quantity, cancellationToken);
        }

        order.UpdateStatus(OrderStatus.Cancelled);
        orderRepository.Update(order);

        return Result.Success();
    }
}