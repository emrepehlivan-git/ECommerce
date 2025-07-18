using ECommerce.Application.Behaviors;
using ECommerce.Application.Mappings;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        services.AddDependencies(assembly);

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TracingBehavior<,>));
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CacheBehavior<,>));
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionalRequestBehavior<,>));
        });

        services.AddMapsterConfiguration();

        return services;
    }
}