using ECommerce.Application.Features.Carts.V1;
using ECommerce.Application.Features.Categories.V1;
using ECommerce.Application.Features.Orders.V1;
using ECommerce.Application.Features.Products.V1;
using ECommerce.Application.Features.Roles.V1;
using ECommerce.Application.Features.UserAddresses.V1;
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
        UserAddressMapperConfig.Configure();
        RoleMapperConfig.Configure();
        CartMapperConfig.Configure();

        return services;
    }
}