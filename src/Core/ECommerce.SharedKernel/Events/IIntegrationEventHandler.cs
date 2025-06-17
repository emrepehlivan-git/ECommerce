using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.SharedKernel.Events;

/// <summary>
/// Defines a handler for a specific integration event type.
/// </summary>
/// <typeparam name="TEvent">Integration event type.</typeparam>
public interface IIntegrationEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
