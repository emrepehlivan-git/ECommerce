using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Helpers;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.SharedKernel;
using FluentValidation;
using MediatR;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Application.Features.Orders.Commands;

public sealed record OrderPlaceCommand(
    Guid UserId,
    Address ShippingAddress,
    Address BillingAddress,
    List<OrderItemRequest> Items) : IRequest<Result<Guid>>, IValidatableRequest, ITransactionalRequest;

public sealed record OrderItemRequest(
    Guid ProductId,
    int Quantity);

public sealed class OrderPlaceCommandValidator : AbstractValidator<OrderPlaceCommand>
{
    public OrderPlaceCommandValidator(
        IProductRepository productRepository,
        IIdentityService identityService,
        LocalizationHelper localizer)
    {
        RuleFor(x => x.UserId)
            .MustAsync(async (id, ct) =>
                await identityService.FindByIdAsync(id) != null)
            .WithMessage(localizer[OrderConsts.UserNotFound]);

        RuleFor(x => x.ShippingAddress)
            .NotNull()
            .WithMessage(localizer[OrderConsts.ShippingAddressRequired]);

        RuleFor(x => x.BillingAddress)
            .NotNull()
            .WithMessage(localizer[OrderConsts.BillingAddressRequired]);

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage(localizer[OrderConsts.EmptyOrder]);

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .MustAsync(async (id, ct) =>
                    await productRepository.AnyAsync(x => x.Id == id, cancellationToken: ct))
                .WithMessage(localizer[OrderConsts.ProductNotFound]);

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage(localizer[OrderConsts.QuantityMustBeGreaterThanZero]);
        });
    }
}

public sealed class OrderPlaceCommandHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IIdentityService identityService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<OrderPlaceCommand, Result<Guid>>(lazyServiceProvider)
{
    public override async Task<Result<Guid>> Handle(OrderPlaceCommand command, CancellationToken cancellationToken)
    {
        if (await identityService.FindByIdAsync(command.UserId) is null)
            return Result.Error(Localizer[OrderConsts.UserNotFound]);

        var order = Order.Create(command.UserId, command.ShippingAddress, command.BillingAddress);

        foreach (var item in command.Items)
        {
            var product = await productRepository.GetByIdAsync(item.ProductId, cancellationToken: cancellationToken);

            if (product is null)
                return Result.Error(Localizer[OrderConsts.ProductNotFound]);

            order.AddItem(item.ProductId, product.Price, item.Quantity);
        }

        orderRepository.Add(order);

        return Result.Success(order.Id);
    }
}