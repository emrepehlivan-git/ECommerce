using ECommerce.Application.Features.Products.DTOs;
using ECommerce.Domain.Entities;
using Mapster;

namespace ECommerce.Application.Features.Products;

public class ProductMapperConfig
{
    public static void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductDto>()
            .Map(dest => dest.Price, src => src.Price != null ? src.Price.Value : 0m)
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : string.Empty)
            .Map(dest => dest.StockQuantity, src => src.Stock != null ? src.Stock.Quantity : 0);
    }
}
