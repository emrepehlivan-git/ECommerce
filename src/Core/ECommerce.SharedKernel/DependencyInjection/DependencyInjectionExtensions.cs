using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace ECommerce.SharedKernel.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDependencies(this IServiceCollection services, params Assembly[] assemblies)
    {
        assemblies = assemblies.Length == 0 ? [Assembly.GetCallingAssembly()] : assemblies;

        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(c => c.AssignableTo<IScopedDependency>())
                .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .AsSelf()
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo<ISingletonDependency>())
                .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .AsSelf()
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
            .AddClasses(c => c.AssignableTo<ITransientDependency>())
                .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .AsSelf()
                .AsImplementedInterfaces()
                .WithTransientLifetime());

        // Explicitly register ILazyServiceProvider
        services.AddTransient<ILazyServiceProvider, LazyServiceProvider>();

        return services;
    }
}
