using ECommerce.Application.Features.Products.V1.DTOs;
using ECommerce.Domain.Entities;
using Mapster;

namespace ECommerce.Application.Features.Products.V1;

public class ProductMapperConfig
{
    public static void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductDto>()
            .Map(dest => dest.Price, src => src.Price != null ? src.Price.Value : 0m)
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : string.Empty)
            .Map(dest => dest.StockQuantity, src => src.Stock != null ? src.Stock.Quantity : 0)
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.Images, src => src.Images.Adapt<List<ProductImageDto>>());
    }
}
