using ECommerce.Application.Features.Orders.V1.DTOs;
using ECommerce.Domain.Entities;
using Mapster;

namespace ECommerce.Application.Features.Orders.V1;

public static class OrderMapperConfig
{
    public static void Configure(TypeAdapterConfig config)
    {
        config.ForType<Order, OrderDto>()
            .Map(dest => dest.Items, src => src.Items)
            .Map(dest => dest.Status, src => src.Status);

        config.ForType<OrderItem, OrderItemDto>()
            .Map(dest => dest.ProductName, src => src.Product.Name);
    }
}