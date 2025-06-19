# RFC-002: Event-Driven Architecture Implementation

**Author**: Development Team  
**Status**: Draft  
**Created**: 2024-12-28  
**Updated**: 2024-12-28  

## Summary

This RFC proposes implementing a comprehensive event-driven architecture for the ECommerce platform to improve system decoupling, enable real-time features, and support future microservice migration.

## Motivation

Current pain points in the system:
- Tight coupling between domain services
- Difficulty in implementing cross-cutting concerns (notifications, audit logs)
- Limited real-time capabilities
- Challenges in implementing complex business workflows
- Need for better system observability and debugging

An event-driven architecture will address these issues by:
- Decoupling domain services through domain events
- Enabling asynchronous processing
- Supporting eventual consistency patterns
- Facilitating integration with external systems
- Improving system resilience and scalability

## Current State Analysis

### Existing Domain Events
The system already has basic domain event infrastructure:

```csharp
// Current implementation
public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}

// Usage in Order entity
public void AddItem(Guid productId, Price unitPrice, int quantity)
{
    // ... business logic
    AddDomainEvent(new StockReservedEvent(productId, quantity));
}
```

### Current Event Handlers
```csharp
// Stock management events
public class StockReservedEventHandler : INotificationHandler<StockReservedEvent>
public class StockNotReservedEventHandler : INotificationHandler<StockNotReservedEvent>
```

## Detailed Design

### 1. Event Types Classification

#### Domain Events (Internal)
Events that represent business state changes within bounded contexts:

```csharp
// Order Events
public sealed record OrderCreatedEvent(
    Guid OrderId,
    Guid UserId,
    decimal TotalAmount,
    DateTime OrderDate
) : IDomainEvent;

public sealed record OrderStatusChangedEvent(
    Guid OrderId,
    OrderStatus FromStatus,
    OrderStatus ToStatus,
    DateTime ChangedAt
) : IDomainEvent;

// Product Events
public sealed record ProductCreatedEvent(
    Guid ProductId,
    string Name,
    decimal Price,
    Guid CategoryId
) : IDomainEvent;

public sealed record ProductPriceChangedEvent(
    Guid ProductId,
    decimal OldPrice,
    decimal NewPrice,
    DateTime ChangedAt
) : IDomainEvent;

// User Events
public sealed record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string FullName,
    DateTime RegisteredAt
) : IDomainEvent;
```

#### Integration Events (External)
Events for communication with external systems or services:

```csharp
public sealed record OrderConfirmationEmailEvent(
    Guid OrderId,
    string CustomerEmail,
    string CustomerName,
    decimal TotalAmount
) : IIntegrationEvent;

public sealed record InventoryUpdateEvent(
    Guid ProductId,
    int QuantityChange,
    string Reason
) : IIntegrationEvent;

public sealed record PaymentProcessedEvent(
    Guid OrderId,
    decimal Amount,
    string PaymentMethod,
    bool IsSuccessful
) : IIntegrationEvent;
```

### 2. Event Infrastructure

#### Enhanced Event Base Classes
```csharp
public abstract record BaseEvent(
    Guid Id,
    DateTime OccurredOn,
    string EventType,
    string Source,
    Dictionary<string, object>? Metadata = null
) : IDomainEvent;

public interface IIntegrationEvent : IDomainEvent
{
    string Version { get; }
    string CorrelationId { get; }
}

public abstract record BaseIntegrationEvent(
    Guid Id,
    DateTime OccurredOn,
    string EventType,
    string Source,
    string Version,
    string CorrelationId,
    Dictionary<string, object>? Metadata = null
) : BaseEvent(Id, OccurredOn, EventType, Source, Metadata), IIntegrationEvent;
```

#### Event Publisher Interface
```csharp
public interface IEventPublisher
{
    Task PublishDomainEventAsync<T>(T domainEvent, CancellationToken cancellationToken = default) 
        where T : IDomainEvent;
    
    Task PublishIntegrationEventAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) 
        where T : IIntegrationEvent;
    
    Task PublishBatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}
```

#### Event Store for Audit and Replay
```csharp
public interface IEventStore
{
    Task SaveEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default);
    Task<IEnumerable<IDomainEvent>> GetEventsByTypeAsync(string eventType, DateTime? fromDate = null, CancellationToken cancellationToken = default);
}

public sealed class EventStoreEntity : BaseEntity
{
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; }
    public Guid? AggregateId { get; set; }
    public string? CorrelationId { get; set; }
    public string Version { get; set; } = "1.0";
}
```

### 3. Event Handler Patterns

#### Immediate Domain Event Handlers
```csharp
public sealed class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing order created event for order {OrderId}", notification.OrderId);
        
        // Send order confirmation email
        await _emailService.SendOrderConfirmationAsync(
            notification.OrderId, 
            notification.UserId, 
            cancellationToken);
    }
}
```

#### Saga Pattern for Complex Workflows
```csharp
public sealed class OrderProcessingSaga : 
    INotificationHandler<OrderCreatedEvent>,
    INotificationHandler<PaymentProcessedEvent>,
    INotificationHandler<InventoryReservedEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventPublisher _eventPublisher;

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Step 1: Reserve inventory
        await _eventPublisher.PublishIntegrationEventAsync(
            new ReserveInventoryEvent(notification.OrderId, notification.Items), 
            cancellationToken);
    }

    public async Task Handle(InventoryReservedEvent notification, CancellationToken cancellationToken)
    {
        // Step 2: Process payment
        await _eventPublisher.PublishIntegrationEventAsync(
            new ProcessPaymentEvent(notification.OrderId, notification.Amount), 
            cancellationToken);
    }

    public async Task Handle(PaymentProcessedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.IsSuccessful)
        {
            // Step 3: Confirm order
            var order = await _orderRepository.GetByIdAsync(notification.OrderId);
            order.UpdateStatus(OrderStatus.Processing);
            await _orderRepository.UpdateAsync(order);
        }
        else
        {
            // Compensate: Release inventory
            await _eventPublisher.PublishIntegrationEventAsync(
                new ReleaseInventoryEvent(notification.OrderId), 
                cancellationToken);
        }
    }
}
```

### 4. Event Publishing Strategy

#### Transactional Outbox Pattern
```csharp
public sealed class TransactionalEventPublisher : IEventPublisher
{
    private readonly ApplicationDbContext _context;
    private readonly IMediator _mediator;
    private readonly IEventStore _eventStore;

    public async Task PublishDomainEventAsync<T>(T domainEvent, CancellationToken cancellationToken = default) 
        where T : IDomainEvent
    {
        // Store event in outbox table
        var outboxEvent = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = typeof(T).Name,
            EventData = JsonSerializer.Serialize(domainEvent),
            OccurredOn = DateTime.UtcNow,
            ProcessedOn = null
        };

        _context.OutboxEvents.Add(outboxEvent);
        await _context.SaveChangesAsync(cancellationToken);

        // Process immediately if in same transaction
        await _mediator.Publish(domainEvent, cancellationToken);
        
        // Mark as processed
        outboxEvent.ProcessedOn = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

#### Background Event Processor
```csharp
public sealed class OutboxEventProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxEventProcessor> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessPendingEventsAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessPendingEventsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var pendingEvents = await context.OutboxEvents
            .Where(e => e.ProcessedOn == null)
            .OrderBy(e => e.OccurredOn)
            .Take(100)
            .ToListAsync(cancellationToken);

        foreach (var outboxEvent in pendingEvents)
        {
            try
            {
                var eventType = Type.GetType(outboxEvent.EventType);
                var domainEvent = JsonSerializer.Deserialize(outboxEvent.EventData, eventType!);
                
                await mediator.Publish(domainEvent!, cancellationToken);
                
                outboxEvent.ProcessedOn = DateTime.UtcNow;
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process outbox event {EventId}", outboxEvent.Id);
                outboxEvent.ProcessedOn = DateTime.UtcNow; // Mark as processed to avoid infinite retry
                outboxEvent.Error = ex.Message;
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
```

### 5. Event Versioning and Schema Evolution

#### Versioned Events
```csharp
public sealed record OrderCreatedEventV1(
    Guid OrderId,
    Guid UserId,
    decimal TotalAmount
) : BaseEvent(Guid.NewGuid(), DateTime.UtcNow, nameof(OrderCreatedEventV1), "Order");

public sealed record OrderCreatedEventV2(
    Guid OrderId,
    Guid UserId,
    decimal TotalAmount,
    string Currency,
    Address ShippingAddress
) : BaseEvent(Guid.NewGuid(), DateTime.UtcNow, nameof(OrderCreatedEventV2), "Order");
```

#### Event Migration Strategy
```csharp
public sealed class EventMigrationService
{
    public IDomainEvent MigrateEvent(string eventType, string eventData, string version)
    {
        return eventType switch
        {
            nameof(OrderCreatedEventV1) when version == "1.0" => 
                MigrateOrderCreatedV1ToV2(JsonSerializer.Deserialize<OrderCreatedEventV1>(eventData)!),
            _ => throw new UnsupportedEventVersionException(eventType, version)
        };
    }

    private OrderCreatedEventV2 MigrateOrderCreatedV1ToV2(OrderCreatedEventV1 oldEvent)
    {
        return new OrderCreatedEventV2(
            oldEvent.OrderId,
            oldEvent.UserId,
            oldEvent.TotalAmount,
            "USD", // Default currency
            new Address("", "", "", "", "") // Default empty address
        );
    }
}
```

### 6. Event Monitoring and Observability

#### Event Metrics
```csharp
public sealed class EventMetricsCollector : INotificationHandler<IDomainEvent>
{
    private readonly IMetricsLogger _metricsLogger;

    public async Task Handle(IDomainEvent notification, CancellationToken cancellationToken)
    {
        _metricsLogger.Counter("domain_events_total")
            .WithTag("event_type", notification.EventType)
            .WithTag("source", notification.Source)
            .Increment();

        _metricsLogger.Histogram("event_processing_duration")
            .WithTag("event_type", notification.EventType)
            .Record(DateTime.UtcNow.Subtract(notification.OccurredOn).TotalMilliseconds);
    }
}
```

#### Event Tracing
```csharp
public sealed class EventTracingHandler<T> : INotificationHandler<T> where T : IDomainEvent
{
    private readonly ILogger<EventTracingHandler<T>> _logger;

    public async Task Handle(T notification, CancellationToken cancellationToken)
    {
        using var activity = Activity.StartActivity($"Event.{typeof(T).Name}");
        activity?.SetTag("event.type", typeof(T).Name);
        activity?.SetTag("event.id", notification.Id.ToString());
        activity?.SetTag("event.source", notification.Source);

        _logger.LogInformation("Event {EventType} processed with ID {EventId}", 
            typeof(T).Name, notification.Id);
    }
}
```

## Implementation Plan

### Phase 1: Foundation (Sprint 1-2)
- [ ] Enhance event base classes and interfaces
- [ ] Implement event store infrastructure
- [ ] Add outbox pattern with database tables
- [ ] Create event publisher implementation
- [ ] Add event metrics and tracing

### Phase 2: Core Events (Sprint 3-4)
- [ ] Define and implement order-related events
- [ ] Define and implement product-related events
- [ ] Define and implement user-related events
- [ ] Implement immediate event handlers
- [ ] Add background event processor

### Phase 3: Complex Workflows (Sprint 5-6)
- [ ] Implement saga patterns for order processing
- [ ] Add compensation logic for failed workflows
- [ ] Implement integration events for external systems
- [ ] Add event replay capabilities

### Phase 4: Advanced Features (Sprint 7-8)
- [ ] Event versioning and migration
- [ ] Event projection for read models
- [ ] Dead letter queue for failed events
- [ ] Event audit and compliance features

## Testing Strategy

### Unit Tests
```csharp
[Fact]
public async Task OrderCreated_ShouldPublishDomainEvent()
{
    // Arrange
    var order = Order.Create(userId, shippingAddress, billingAddress);
    
    // Act
    order.AddItem(productId, unitPrice, quantity);
    
    // Assert
    order.DomainEvents.Should().ContainSingle(e => e is StockReservedEvent);
}
```

### Integration Tests
```csharp
[Fact]
public async Task EventPublisher_ShouldStoreEventInOutbox()
{
    // Arrange
    var domainEvent = new OrderCreatedEvent(orderId, userId, totalAmount, DateTime.UtcNow);
    
    // Act
    await _eventPublisher.PublishDomainEventAsync(domainEvent);
    
    // Assert
    var outboxEvent = await _context.OutboxEvents.FirstOrDefaultAsync();
    outboxEvent.Should().NotBeNull();
    outboxEvent!.EventType.Should().Be(nameof(OrderCreatedEvent));
}
```

### Event Replay Tests
```csharp
[Fact]
public async Task EventStore_CanReplayEvents()
{
    // Arrange
    var events = new List<IDomainEvent> { event1, event2, event3 };
    foreach (var evt in events)
        await _eventStore.SaveEventAsync(evt);
    
    // Act
    var replayedEvents = await _eventStore.GetEventsAsync(aggregateId);
    
    // Assert
    replayedEvents.Should().HaveCount(3);
    replayedEvents.Should().BeInOrder(e => e.OccurredOn);
}
```

## Risks and Mitigation

### Risk: Event Ordering Issues
**Mitigation**: Use event sourcing with aggregate sequence numbers, partition by aggregate ID

### Risk: Eventual Consistency Complexity
**Mitigation**: Clear documentation, saga patterns, compensation actions

### Risk: Event Schema Evolution
**Mitigation**: Versioning strategy, backward compatibility, migration tools

### Risk: Performance Impact
**Mitigation**: Async processing, batching, monitoring, circuit breakers

## Success Metrics

- Event processing latency < 100ms (95th percentile)
- Event delivery reliability > 99.9%
- Zero data loss in event processing
- Reduced coupling between domain services
- Improved system observability and debugging

## Future Considerations

- Message brokers (RabbitMQ, Azure Service Bus) for external integration
- Event sourcing for complete audit trails
- CQRS read models driven by events
- Multi-tenant event isolation
- Event-driven microservice communication

---

**Next Steps**:
1. Technical spike for outbox pattern implementation
2. Define initial set of critical domain events
3. Implement event store infrastructure
4. Create monitoring and alerting for event processing 