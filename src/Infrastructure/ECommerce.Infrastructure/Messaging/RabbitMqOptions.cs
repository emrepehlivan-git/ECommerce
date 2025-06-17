namespace ECommerce.Infrastructure.Messaging;

public sealed record RabbitMqOptions
{
    public string ConnectionString { get; init; } = string.Empty;
}
