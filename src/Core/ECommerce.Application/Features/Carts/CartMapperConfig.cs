using ECommerce.Application.Features.Carts.DTOs;
using ECommerce.Domain.Entities;
using Mapster;

namespace ECommerce.Application.Features.Carts;

public static class CartMapperConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Cart, CartDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.Items, src => src.Items.Adapt<List<CartItemDto>>())
            .Map(dest => dest.TotalAmount, src => src.TotalAmount)
            .Map(dest => dest.TotalItems, src => src.TotalItems)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt);

        TypeAdapterConfig<CartItem, CartItemDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.ProductName, src => src.Product.Name)
            .Map(dest => dest.UnitPrice, src => src.UnitPrice)
            .Map(dest => dest.Quantity, src => src.Quantity)
            .Map(dest => dest.TotalPrice, src => src.TotalPrice);

        TypeAdapterConfig<Cart, CartSummaryDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.TotalItems, src => src.TotalItems)
            .Map(dest => dest.TotalAmount, src => src.TotalAmount);
    }
} 