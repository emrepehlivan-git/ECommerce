namespace ECommerce.Application.Interfaces;

public interface IEventBusFactory
{
    IEventBus Create(IServiceProvider serviceProvider);
}
