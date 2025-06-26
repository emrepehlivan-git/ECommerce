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

public sealed record OrderItemAddCommand(
    Guid OrderId,
    Guid ProductId,
    int Quantity) : IRequest<Result>, IValidatableRequest, ITransactionalRequest;

public sealed class OrderItemAddCommandValidator : AbstractValidator<OrderItemAddCommand>
{
    public OrderItemAddCommandValidator(
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

                return order is not null && order.Status == OrderStatus.Pending;
            })
            .WithMessage(localizer[OrderConsts.OrderCannotBeModified]);

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage(localizer[OrderConsts.QuantityMustBeGreaterThanZero]);
    }
}

public sealed class OrderItemAddCommandHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<OrderItemAddCommand, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(OrderItemAddCommand command, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(command.OrderId, cancellationToken: cancellationToken);

        var product = await productRepository.GetByIdAsync(command.ProductId, 
            include: x => x.Include(p => p.Stock), 
            cancellationToken: cancellationToken);

        if (product is null)
            return Result.NotFound(Localizer[OrderConsts.ProductNotFound]);

        order!.AddItem(command.ProductId, product.Price, command.Quantity);

        orderRepository.Update(order);

        return Result.Success();
    }
}