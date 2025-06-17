namespace ECommerce.Application.Interfaces;

using ECommerce.SharedKernel.Events;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent;
}
