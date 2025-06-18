using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.SharedKernel.DependencyInjection;

public sealed class LazyServiceProvider : ILazyServiceProvider, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, Lazy<object?>> _lazyServices;

    public LazyServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _lazyServices = new ConcurrentDictionary<Type, Lazy<object?>>();
    }

    public T LazyGetRequiredService<T>()
    {
        return (T)_lazyServices.GetOrAdd(typeof(T), t => new Lazy<object?>(() => _serviceProvider.GetRequiredService(t))).Value!;
    }

    public object LazyGetRequiredService(Type serviceType)
    {
        return LazyGetRequiredService(serviceType);
    }

    public T? LazyGetService<T>()
    {
        return (T)LazyGetRequiredService(typeof(T));
    }

    public object? LazyGetService(Type serviceType)
    {
        return _lazyServices.GetOrAdd(serviceType, t => new Lazy<object?>(() => _serviceProvider.GetService(t))).Value;
    }

    public T LazyGetService<T>(T defaultValue)
    {
        return (T)LazyGetService(typeof(T), defaultValue!);
    }

    public object LazyGetService(Type serviceType, object defaultValue)
    {
        return LazyGetService(serviceType) ?? defaultValue;
    }

    public object LazyGetService(Type serviceType, Func<IServiceProvider, object> factory)
    {
        return _lazyServices.GetOrAdd(serviceType, t => new Lazy<object?>(() => factory(_serviceProvider))).Value!;
    }

    public T LazyGetService<T>(Func<IServiceProvider, object> factory)
    {
        return (T)LazyGetService(typeof(T), factory);
    }
}
