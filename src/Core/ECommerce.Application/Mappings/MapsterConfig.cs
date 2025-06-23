using ECommerce.Application.Features.Carts;
using ECommerce.Application.Features.Categories;
using ECommerce.Application.Features.Orders;
using ECommerce.Application.Features.Products;
using ECommerce.Application.Features.Roles;
using ECommerce.Application.Features.UserAddresses;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Application.Mappings;

public static class MapsterConfig
{
    public static IServiceCollection AddMapsterConfiguration(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;

        config.Scan(typeof(MapsterConfig).Assembly);

        ProductMapperConfig.Configure(config);
        CategoryMapperConfig.Configure(config);
        OrderMapperConfig.Configure(config);
        UserAddressMapperConfig.Configure(config);
        RoleMapperConfig.Configure();
        CartMapperConfig.Configure();

        return services;
    }
}