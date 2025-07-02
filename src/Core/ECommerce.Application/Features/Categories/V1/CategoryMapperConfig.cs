using ECommerce.Application.Features.Categories.V1.DTOs;
using ECommerce.Domain.Entities;
using Mapster;

namespace ECommerce.Application.Features.Categories.V1;

public class CategoryMapperConfig
{
    public static void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<Category, CategoryDto>();
    }
}