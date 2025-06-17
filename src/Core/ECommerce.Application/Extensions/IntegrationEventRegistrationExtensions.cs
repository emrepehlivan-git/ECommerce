using ECommerce.SharedKernel.Events;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace ECommerce.Application.Extensions;

public static class IntegrationEventRegistrationExtensions
{
    public static IServiceCollection AddIntegrationEventHandlers(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblies(typeof(IntegrationEventRegistrationExtensions).Assembly)
            .AddClasses(c => c.AssignableTo(typeof(IIntegrationEventHandler<>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        return services;
    }
}
