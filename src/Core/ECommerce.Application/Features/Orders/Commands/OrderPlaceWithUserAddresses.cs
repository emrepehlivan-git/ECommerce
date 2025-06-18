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

namespace ECommerce.Application.Features.Orders.Commands;

public sealed record OrderPlaceWithUserAddressesCommand(
    Guid UserId,
    Guid? ShippingAddressId,
    Guid? BillingAddressId,
    bool UseSameForBilling,
    List<OrderItemRequest> Items) : IRequest<Result<Guid>>, IValidatableRequest, ITransactionalRequest;

public sealed class OrderPlaceWithUserAddressesCommandValidator : AbstractValidator<OrderPlaceWithUserAddressesCommand>
{
    public OrderPlaceWithUserAddressesCommandValidator(
        IProductRepository productRepository,
        IIdentityService identityService,
        IUserAddressRepository userAddressRepository,
        LocalizationHelper localizer)
    {
        RuleFor(x => x.UserId)
            .MustAsync(async (id, ct) =>
                await identityService.FindByIdAsync(id) != null)
            .WithMessage(localizer[OrderConsts.UserNotFound]);

        RuleFor(x => x.ShippingAddressId)
            .NotNull()
            .When(x => !x.UseSameForBilling || x.BillingAddressId == null)
            .WithMessage(localizer[OrderConsts.ShippingAddressRequired]);

        RuleFor(x => x.ShippingAddressId)
            .MustAsync(async (command, addressId, ct) =>
            {
                if (addressId == null) return false;
                return await userAddressRepository.AnyAsync(x => x.Id == addressId.Value && x.UserId == command.UserId && x.IsActive, ct);
            })
            .When(x => x.ShippingAddressId != null)
            .WithMessage("Invalid shipping address");

        RuleFor(x => x.BillingAddressId)
            .NotNull()
            .When(x => !x.UseSameForBilling)
            .WithMessage(localizer[OrderConsts.BillingAddressRequired]);

        RuleFor(x => x.BillingAddressId)
            .MustAsync(async (command, addressId, ct) =>
            {
                if (addressId == null) return false;
                return await userAddressRepository.AnyAsync(x => x.Id == addressId.Value && x.UserId == command.UserId && x.IsActive, ct);
            })
            .When(x => !x.UseSameForBilling && x.BillingAddressId != null)
            .WithMessage("Invalid billing address");

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

public sealed class OrderPlaceWithUserAddressesCommandHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IUserAddressRepository userAddressRepository,
    IIdentityService identityService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<OrderPlaceWithUserAddressesCommand, Result<Guid>>(lazyServiceProvider)
{
    public override async Task<Result<Guid>> Handle(OrderPlaceWithUserAddressesCommand command, CancellationToken cancellationToken)
    {
        if (await identityService.FindByIdAsync(command.UserId) is null)
            return Result.Error(Localizer[OrderConsts.UserNotFound]);

        // Shipping address alma
        UserAddress? shippingAddress = null;
        if (command.ShippingAddressId.HasValue)
        {
            shippingAddress = await userAddressRepository.GetByIdAsync(command.ShippingAddressId.Value, cancellationToken: cancellationToken);
        }
        else
        {
            // Varsayılan adresi kullan
            shippingAddress = await userAddressRepository.GetDefaultAddressAsync(command.UserId, cancellationToken);
        }

        if (shippingAddress is null || shippingAddress.UserId != command.UserId || !shippingAddress.IsActive)
            return Result.Error(Localizer[OrderConsts.ShippingAddressRequired]);

        // Billing address alma
        UserAddress? billingAddress = null;
        if (command.UseSameForBilling)
        {
            billingAddress = shippingAddress;
        }
        else if (command.BillingAddressId.HasValue)
        {
            billingAddress = await userAddressRepository.GetByIdAsync(command.BillingAddressId.Value, cancellationToken: cancellationToken);
        }
        else
        {
            // Varsayılan adresi kullan
            billingAddress = await userAddressRepository.GetDefaultAddressAsync(command.UserId, cancellationToken);
        }

        if (billingAddress is null || billingAddress.UserId != command.UserId || !billingAddress.IsActive)
            return Result.Error(Localizer[OrderConsts.BillingAddressRequired]);

        var order = Order.Create(command.UserId, shippingAddress.Address, billingAddress.Address);

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