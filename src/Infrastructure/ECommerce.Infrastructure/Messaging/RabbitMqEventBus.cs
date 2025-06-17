using System.Text;
using System.Text.Json;
using ECommerce.Application.Interfaces;
using ECommerce.SharedKernel.Events;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ECommerce.Infrastructure.Messaging;

public sealed class RabbitMqEventBus(IOptions<RabbitMqOptions> options) : IEventBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqEventBus(IOptions<RabbitMqOptions> options)
        : this(new ConnectionFactory { Uri = new Uri(options.Value.ConnectionString) })
    {
    }

    private RabbitMqEventBus(IConnectionFactory factory)
    {
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class, IIntegrationEvent
    {
        var exchange = typeof(TEvent).Name;
        EnsureExchange(exchange);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));
        var properties = _channel.CreateBasicProperties();
        properties.DeliveryMode = 2; // persistent

        _channel.BasicPublish(exchange, routingKey: string.Empty, properties: properties, body: body);
        return Task.CompletedTask;
    }

    private void EnsureExchange(string exchange)
    {
        _channel.ExchangeDeclare(exchange, ExchangeType.Fanout, durable: true, autoDelete: false);
    }

    public void CreateQueue(string queueName)
    {
        _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
    }

    public void BindQueue(string queueName, string exchange)
    {
        _channel.QueueBind(queueName, exchange, routingKey: string.Empty);
    }

    public void Dispose()
    {
        _channel.Close();
        _connection.Close();
    }
}
