using ECommerce.SharedKernel.Events;

namespace ECommerce.Domain.UnitTests.Entities;

public sealed class BaseEntityTests
{
    private sealed class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestDomainEvent : IDomainEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public string EventType { get; } = nameof(TestDomainEvent);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithEmptyId()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        entity.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithEmptyDomainEvents()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        entity.DomainEvents.Should().NotBeNull();
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Id_ShouldBeSettable()
    {
        // Arrange
        var entity = new TestEntity();
        var expectedId = Guid.NewGuid();

        // Act
        entity.Id = expectedId;

        // Assert
        entity.Id.Should().Be(expectedId);
    }

    [Fact]
    public void AddDomainEvent_ShouldAddEventToDomainEvents()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();

        // Act
        entity.AddDomainEvent(domainEvent);

        // Assert
        entity.DomainEvents.Should().HaveCount(1);
        entity.DomainEvents.Should().Contain(domainEvent);
    }

    [Fact]
    public void AddDomainEvent_WithMultipleEvents_ShouldAddAllEvents()
    {
        // Arrange
        var entity = new TestEntity();
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();

        // Act
        entity.AddDomainEvent(event1);
        entity.AddDomainEvent(event2);

        // Assert
        entity.DomainEvents.Should().HaveCount(2);
        entity.DomainEvents.Should().Contain(event1);
        entity.DomainEvents.Should().Contain(event2);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var entity = new TestEntity();
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();
        entity.AddDomainEvent(event1);
        entity.AddDomainEvent(event2);

        // Act
        entity.ClearDomainEvents();

        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_WithEmptyEvents_ShouldNotThrow()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        var act = () => entity.ClearDomainEvents();

        // Assert
        act.Should().NotThrow();
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();
        entity.AddDomainEvent(domainEvent);

        // Act
        var domainEvents = entity.DomainEvents;

        // Assert
        domainEvents.Should().BeAssignableTo<IReadOnlyCollection<IDomainEvent>>();
        domainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void AddDomainEvent_WithSameEvent_ShouldAddBothInstances()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();

        // Act
        entity.AddDomainEvent(domainEvent);
        entity.AddDomainEvent(domainEvent);

        // Assert
        entity.DomainEvents.Should().HaveCount(2);
        entity.DomainEvents.Should().AllBeEquivalentTo(domainEvent);
    }
} 