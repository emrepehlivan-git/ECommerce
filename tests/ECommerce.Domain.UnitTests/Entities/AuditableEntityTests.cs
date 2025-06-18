namespace ECommerce.Domain.UnitTests.Entities;

public sealed class AuditableEntityTests
{
    private sealed class TestAuditableEntity : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var entity = new TestAuditableEntity();

        // Assert
        entity.Id.Should().Be(Guid.Empty);
        entity.CreatedAt.Should().Be(default);
        entity.CreatedBy.Should().BeNull();
        entity.UpdatedAt.Should().BeNull();
        entity.UpdatedBy.Should().BeNull();
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void CreatedAt_ShouldBeSettable()
    {
        // Arrange
        var entity = new TestAuditableEntity();
        var expectedDate = DateTime.UtcNow;

        // Act
        entity.CreatedAt = expectedDate;

        // Assert
        entity.CreatedAt.Should().Be(expectedDate);
    }

    [Fact]
    public void CreatedBy_ShouldBeSettable()
    {
        // Arrange
        var entity = new TestAuditableEntity();
        var expectedUserId = Guid.NewGuid();

        // Act
        entity.CreatedBy = expectedUserId;

        // Assert
        entity.CreatedBy.Should().Be(expectedUserId);
    }

    [Fact]
    public void UpdatedAt_ShouldBeSettable()
    {
        // Arrange
        var entity = new TestAuditableEntity();
        var expectedDate = DateTime.UtcNow;

        // Act
        entity.UpdatedAt = expectedDate;

        // Assert
        entity.UpdatedAt.Should().Be(expectedDate);
    }

    [Fact]
    public void UpdatedBy_ShouldBeSettable()
    {
        // Arrange
        var entity = new TestAuditableEntity();
        var expectedUserId = Guid.NewGuid();

        // Act
        entity.UpdatedBy = expectedUserId;

        // Assert
        entity.UpdatedBy.Should().Be(expectedUserId);
    }

    [Fact]
    public void AuditableEntity_ShouldInheritFromBaseEntity()
    {
        // Arrange
        var entity = new TestAuditableEntity();

        // Assert
        entity.Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void AuditableEntity_ShouldImplementIAuditableEntity()
    {
        // Arrange
        var entity = new TestAuditableEntity();

        // Assert
        entity.Should().BeAssignableTo<ECommerce.SharedKernel.Entities.IAuditableEntity>();
    }

    [Fact]
    public void SetAuditProperties_ShouldSetAllProperties()
    {
        // Arrange
        var entity = new TestAuditableEntity();
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var createdBy = Guid.NewGuid();
        var updatedAt = DateTime.UtcNow;
        var updatedBy = Guid.NewGuid();

        // Act
        entity.CreatedAt = createdAt;
        entity.CreatedBy = createdBy;
        entity.UpdatedAt = updatedAt;
        entity.UpdatedBy = updatedBy;

        // Assert
        entity.CreatedAt.Should().Be(createdAt);
        entity.CreatedBy.Should().Be(createdBy);
        entity.UpdatedAt.Should().Be(updatedAt);
        entity.UpdatedBy.Should().Be(updatedBy);
    }

    [Fact]
    public void CreatedBy_WithNullValue_ShouldBeNull()
    {
        // Arrange
        var entity = new TestAuditableEntity();

        // Act
        entity.CreatedBy = null;

        // Assert
        entity.CreatedBy.Should().BeNull();
    }

    [Fact]
    public void UpdatedBy_WithNullValue_ShouldBeNull()
    {
        // Arrange
        var entity = new TestAuditableEntity();

        // Act
        entity.UpdatedBy = null;

        // Assert
        entity.UpdatedBy.Should().BeNull();
    }
} 