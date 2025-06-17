using ECommerce.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Messaging;

/// <summary>
/// Factory for creating the configured <see cref="IEventBus"/> implementation.
/// Currently only supports RabbitMQ.
/// </summary>
public sealed class EventBusFactory : IEventBusFactory
{
    private readonly EventBusProvider _provider;

    public EventBusFactory(IConfiguration configuration)
    {
        var providerString = configuration["EventBus:Provider"] ?? nameof(EventBusProvider.RabbitMQ);
        if (!Enum.TryParse(providerString, ignoreCase: true, out EventBusProvider parsed))
        {
            parsed = EventBusProvider.RabbitMQ;
        }
        _provider = parsed;
    }

    public IEventBus Create(IServiceProvider serviceProvider)
    {
        return _provider switch
        {
            EventBusProvider.RabbitMQ => new RabbitMqEventBus(serviceProvider.GetRequiredService<IOptions<RabbitMqOptions>>()),
            _ => throw new NotSupportedException($"Event bus provider '{_provider}' is not supported")
        };
    }
}
