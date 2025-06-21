using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Helpers;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Repositories;
using ECommerce.Application.Exceptions;
using ECommerce.Domain.Entities;
using FluentValidation;
using MediatR;
using ECommerce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using ECommerce.Application.Features.Orders.Specifications;
using ECommerce.Application.Services;

namespace ECommerce.Application.Features.Orders.Commands;

public sealed record OrderPlaceCommand(
    Guid UserId,
    Address? ShippingAddress,
    Address? BillingAddress,
    Guid? ShippingAddressId,
    Guid? BillingAddressId,
    bool UseSameForBilling,
    List<OrderItemRequest> Items) : IRequest<Result<Guid>>, IValidatableRequest, ITransactionalRequest;

public sealed record OrderItemRequest(
    Guid ProductId,
    int Quantity);

public sealed class OrderPlaceCommandValidator : AbstractValidator<OrderPlaceCommand>
{
    public OrderPlaceCommandValidator(
        IProductRepository productRepository,
        IUserService userService,
        IUserAddressRepository userAddressRepository,
        LocalizationHelper localizer)
    {
        RuleFor(x => x.UserId)
            .MustAsync(async (id, ct) =>
                await userService.FindByIdAsync(id) != null)
            .WithMessage(localizer[OrderConsts.UserNotFound]);

        RuleFor(x => x)
            .Must(x => (x.ShippingAddress != null && x.BillingAddress != null) || 
                      (x.ShippingAddressId != null && (x.BillingAddressId != null || x.UseSameForBilling)))
            .WithMessage(localizer[OrderConsts.ShippingAddressRequired]);

        RuleFor(x => x.ShippingAddress)
            .NotNull()
            .When(x => x.ShippingAddressId == null)
            .WithMessage(localizer[OrderConsts.ShippingAddressRequired]);

        RuleFor(x => x.BillingAddress)
            .NotNull()
            .When(x => x.BillingAddressId == null && !x.UseSameForBilling)
            .WithMessage(localizer[OrderConsts.BillingAddressRequired]);

        RuleFor(x => x.ShippingAddressId)
            .NotNull()
            .When(x => x.ShippingAddress == null)
            .WithMessage(localizer[OrderConsts.ShippingAddressRequired]);

        RuleFor(x => x.ShippingAddressId)
            .MustAsync(async (command, addressId, ct) =>
            {
                if (addressId == null) return true;
                return await userAddressRepository.AnyAsync(x => x.Id == addressId.Value && x.UserId == command.UserId && x.IsActive, ct);
            })
            .When(x => x.ShippingAddressId != null)
            .WithMessage(localizer[OrderConsts.ShippingAddressRequired]);

        RuleFor(x => x.BillingAddressId)
            .NotNull()
            .When(x => x.BillingAddress == null && !x.UseSameForBilling)
            .WithMessage(localizer[OrderConsts.BillingAddressRequired]);

        RuleFor(x => x.BillingAddressId)
            .MustAsync(async (command, addressId, ct) =>
            {
                if (addressId == null) return true;
                return await userAddressRepository.AnyAsync(x => x.Id == addressId.Value && x.UserId == command.UserId && x.IsActive, ct);
            })
            .When(x => !x.UseSameForBilling && x.BillingAddressId != null)
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

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .MustAsync(async (id, ct) =>
                {
                    var spec = new ProductOrderableSpecification(id);
                    return await productRepository.AnyAsync(spec, cancellationToken: ct);
                })
                .WithMessage(localizer[OrderConsts.ProductNotActive]);
        });
    }
}

public sealed class OrderPlaceCommandHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IUserAddressRepository userAddressRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<OrderPlaceCommand, Result<Guid>>(lazyServiceProvider)
{
    public override async Task<Result<Guid>> Handle(OrderPlaceCommand command, CancellationToken cancellationToken)
    {
        Address shippingAddress;
        Address billingAddress;

        if (command.ShippingAddress != null && command.BillingAddress != null)
        {
            shippingAddress = command.ShippingAddress;
            billingAddress = command.BillingAddress;
        }
        else
        {
            var shippingAddressEntity = await userAddressRepository.GetByIdAsync(command.ShippingAddressId!.Value, cancellationToken: cancellationToken);
            if (shippingAddressEntity is null)
                return Result.Error(Localizer[OrderConsts.ShippingAddressNotFound]);

            var billingAddressId = command.UseSameForBilling ? command.ShippingAddressId!.Value : command.BillingAddressId!.Value;
            var billingAddressEntity = await userAddressRepository.GetByIdAsync(billingAddressId, cancellationToken: cancellationToken);
            if (billingAddressEntity is null)
                return Result.Error(Localizer[OrderConsts.BillingAddressNotFound]);

            shippingAddress = shippingAddressEntity.Address;
            billingAddress = billingAddressEntity.Address;
        }

        var productIds = command.Items.Select(i => i.ProductId).ToList();
        var spec = new ProductOrderableSpecification(productIds);
        var products = await productRepository.ListAsync(spec, cancellationToken);

        var order = Order.Create(command.UserId, shippingAddress, billingAddress);

        foreach (var item in command.Items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
            
            if (product is null)
                return Result.Error(Localizer[OrderConsts.ProductNotFound]);

            if (!product.IsActive)
                return Result.Error(Localizer[OrderConsts.ProductNotActive]);

            if (!product.HasSufficientStock(item.Quantity))
                return Result.Error(Localizer[OrderConsts.InsufficientStock]);

            order.AddItem(item.ProductId, product.Price, item.Quantity);
        }

        orderRepository.Add(order);

        return Result.Success(order.Id);
    }
}