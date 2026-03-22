namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

// Test entities
public class MapWhenTestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public MapWhenOrderStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal? Price { get; set; }
    public int Age { get; set; }
    public string? Email { get; set; }
}

public enum MapWhenOrderStatus
{
    Pending,
    Processing,
    Completed,
    Cancelled
}

// Basic boolean condition test
[MappingTarget<MapWhenTestEntity>()]
public partial class MapWhenBooleanTarget
{
    [MapWhen("IsActive")]
    public string? Email { get; set; }
}

// Equality comparison test
[MappingTarget<MapWhenTestEntity>()]
public partial class MapWhenEqualityTarget
{
    [MapWhen("Status == MapWhenOrderStatus.Completed")]
    public DateTime? CompletedAt { get; set; }
}

// Null check test
[MappingTarget<MapWhenTestEntity>()]
public partial class MapWhenNullCheckTarget
{
    [MapWhen("Email != null")]
    public string? Email { get; set; }
}

// With default value test
[MappingTarget<MapWhenTestEntity>()]
public partial class MapWhenDefaultValueTarget
{
    [MapWhen("Price != null")]
    public decimal? Price { get; set; }
}

// Comparison operator test
[MappingTarget<MapWhenTestEntity>()]
public partial class MapWhenComparisonTarget
{
    [MapWhen("Age >= 18")]
    public string? Email { get; set; }
}

// Multiple conditions (AND logic) test
[MappingTarget<MapWhenTestEntity>()]
public partial class MapWhenMultipleConditionsTarget
{
    [MapWhen("IsActive")]
    [MapWhen("Status == MapWhenOrderStatus.Completed")]
    public DateTime? CompletedAt { get; set; }
}

// Combined with other properties
[MappingTarget<MapWhenTestEntity>()]
public partial class MapWhenMixedTarget
{
    [MapWhen("IsActive")]
    public string? Email { get; set; }

    [MapWhen("Status == MapWhenOrderStatus.Completed")]
    public DateTime? CompletedAt { get; set; }
}

// Exclude from projection test
[MappingTarget<MapWhenTestEntity>()]
public partial class MapWhenNoProjectionTarget
{
    [MapWhen("IsActive", IncludeInProjection = false)]
    public string? Email { get; set; }
}

// Negation test
[MappingTarget<MapWhenTestEntity>()]
public partial class MapWhenNegationTarget
{
    [MapWhen("!IsActive")]
    public string? Email { get; set; }
}

// Not equal test
[MappingTarget<MapWhenTestEntity>()]
public partial class MapWhenNotEqualTarget
{
    [MapWhen("Status != MapWhenOrderStatus.Cancelled")]
    public DateTime? CompletedAt { get; set; }
}

public class MapWhenTests
{
    [Fact]
    public void Constructor_ShouldMapWhenBooleanConditionIsTrue()
    {
        // Arrange
        var entity = new MapWhenTestEntity
        {
            Id = 1,
            Name = "Test",
            IsActive = true,
            Email = "test@example.com"
        };

        // Act
        var target = new MapWhenBooleanTarget(entity);

        // Assert
        target.Id.ShouldBe(1);
        target.Name.ShouldBe("Test");
        target.Email.ShouldBe("test@example.com");
    }

    [Fact]
    public void Constructor_ShouldNotMapWhenBooleanConditionIsFalse()
    {
        // Arrange
        var entity = new MapWhenTestEntity
        {
            Id = 1,
            Name = "Test",
            IsActive = false,
            Email = "test@example.com"
        };

        // Act
        var target = new MapWhenBooleanTarget(entity);

        // Assert
        target.Id.ShouldBe(1);
        target.Name.ShouldBe("Test");
        target.Email.ShouldBeNull(); // Condition false, so default
    }

    [Fact]
    public void Constructor_ShouldMapWhenEqualityConditionIsTrue()
    {
        // Arrange
        var completedTime = new DateTime(2024, 1, 15, 10, 30, 0);
        var entity = new MapWhenTestEntity
        {
            Id = 1,
            Name = "Order",
            Status = MapWhenOrderStatus.Completed,
            CompletedAt = completedTime
        };

        // Act
        var target = new MapWhenEqualityTarget(entity);

        // Assert
        target.Id.ShouldBe(1);
        target.CompletedAt.ShouldBe(completedTime);
    }

    [Fact]
    public void Constructor_ShouldNotMapWhenEqualityConditionIsFalse()
    {
        // Arrange
        var entity = new MapWhenTestEntity
        {
            Id = 1,
            Name = "Order",
            Status = MapWhenOrderStatus.Pending,
            CompletedAt = DateTime.Now
        };

        // Act
        var target = new MapWhenEqualityTarget(entity);

        // Assert
        target.Id.ShouldBe(1);
        target.CompletedAt.ShouldBeNull();
    }

    [Fact]
    public void Constructor_ShouldMapWhenNullCheckPasses()
    {
        // Arrange
        var entity = new MapWhenTestEntity
        {
            Id = 1,
            Email = "test@example.com"
        };

        // Act
        var target = new MapWhenNullCheckTarget(entity);

        // Assert
        target.Email.ShouldBe("test@example.com");
    }

    [Fact]
    public void Constructor_ShouldNotMapWhenNullCheckFails()
    {
        // Arrange
        var entity = new MapWhenTestEntity
        {
            Id = 1,
            Email = null
        };

        // Act
        var target = new MapWhenNullCheckTarget(entity);

        // Assert
        target.Email.ShouldBeNull();
    }

    [Fact]
    public void Constructor_ShouldReturnDefaultWhenConditionIsFalse()
    {
        // Arrange
        var entity = new MapWhenTestEntity
        {
            Id = 1,
            Price = null
        };

        // Act
        var target = new MapWhenDefaultValueTarget(entity);

        // Assert
        target.Price.ShouldBeNull();
    }

    [Fact]
    public void Constructor_ShouldMapWhenConditionIsTrue_WithNullableProperty()
    {
        // Arrange
        var entity = new MapWhenTestEntity
        {
            Id = 1,
            Price = 99.99m
        };

        // Act
        var target = new MapWhenDefaultValueTarget(entity);

        // Assert
        target.Price.ShouldBe(99.99m);
    }

    [Fact]
    public void Constructor_ShouldMapWhenComparisonConditionIsTrue()
    {
        // Arrange
        var entity = new MapWhenTestEntity
        {
            Id = 1,
            Age = 21,
            Email = "adult@example.com"
        };

        // Act
        var target = new MapWhenComparisonTarget(entity);

        // Assert
        target.Email.ShouldBe("adult@example.com");
    }

    [Fact]
    public void Constructor_ShouldNotMapWhenComparisonConditionIsFalse()
    {
        // Arrange
        var entity = new MapWhenTestEntity
        {
            Id = 1,
            Age = 16,
            Email = "minor@example.com"
        };

        // Act
        var target = new MapWhenComparisonTarget(entity);

        // Assert
        target.Email.ShouldBeNull();
    }

    [Fact]
    public void Constructor_ShouldMapWhenAllMultipleConditionsAreTrue()
    {
        // Arrange
        var completedTime = new DateTime(2024, 1, 15, 10, 30, 0);
        var entity = new MapWhenTestEntity
        {
            Id = 1,
            IsActive = true,
            Status = MapWhenOrderStatus.Completed,
            CompletedAt = completedTime
        };

        // Act
        var target = new MapWhenMultipleConditionsTarget(entity);

        // Assert
        target.CompletedAt.ShouldBe(completedTime);
    }

    [Fact]
    public void Constructor_ShouldNotMapWhenAnyMultipleConditionIsFalse()
    {
        // Arrange - IsActive is false
        var entity = new MapWhenTestEntity
        {
            Id = 1,
            IsActive = false,
            Status = MapWhenOrderStatus.Completed,
            CompletedAt = DateTime.Now
        };

        // Act
        var target = new MapWhenMultipleConditionsTarget(entity);

        // Assert
        target.CompletedAt.ShouldBeNull();
    }

    [Fact]
    public void Projection_ShouldApplyConditions()
    {
        // Arrange
        var entities = new[]
        {
            new MapWhenTestEntity { Id = 1, Name = "Active", IsActive = true, Email = "active@example.com" },
            new MapWhenTestEntity { Id = 2, Name = "Inactive", IsActive = false, Email = "inactive@example.com" }
        }.AsQueryable();

        // Act
        var targets = entities.Select(MapWhenBooleanTarget.Projection).ToList();

        // Assert
        targets.Count().ShouldBe(2);
        targets[0].Email.ShouldBe("active@example.com");
        targets[1].Email.ShouldBeNull();
    }

    [Fact]
    public void Projection_ShouldApplyEqualityConditions()
    {
        // Arrange
        var completedTime = new DateTime(2024, 1, 15);
        var entities = new[]
        {
            new MapWhenTestEntity { Id = 1, Status = MapWhenOrderStatus.Completed, CompletedAt = completedTime },
            new MapWhenTestEntity { Id = 2, Status = MapWhenOrderStatus.Pending, CompletedAt = completedTime }
        }.AsQueryable();

        // Act
        var targets = entities.Select(MapWhenEqualityTarget.Projection).ToList();

        // Assert
        targets[0].CompletedAt.ShouldBe(completedTime);
        targets[1].CompletedAt.ShouldBeNull();
    }

    [Fact]
    public void Constructor_ShouldMapMixedProperties()
    {
        // Arrange
        var completedTime = new DateTime(2024, 1, 15);
        var entity = new MapWhenTestEntity
        {
            Id = 1,
            Name = "Test Order",
            IsActive = true,
            Status = MapWhenOrderStatus.Completed,
            Email = "test@example.com",
            CompletedAt = completedTime
        };

        // Act
        var target = new MapWhenMixedTarget(entity);

        // Assert
        target.Id.ShouldBe(1);
        target.Name.ShouldBe("Test Order");
        target.Email.ShouldBe("test@example.com");
        target.CompletedAt.ShouldBe(completedTime);
    }

    [Fact]
    public void Constructor_ShouldMapWhenNegationConditionIsTrue()
    {
        // Arrange - !IsActive is true when IsActive is false
        var entity = new MapWhenTestEntity
        {
            Id = 1,
            IsActive = false,
            Email = "inactive@example.com"
        };

        // Act
        var target = new MapWhenNegationTarget(entity);

        // Assert
        target.Email.ShouldBe("inactive@example.com");
    }

    [Fact]
    public void Constructor_ShouldNotMapWhenNegationConditionIsFalse()
    {
        // Arrange - !IsActive is false when IsActive is true
        var entity = new MapWhenTestEntity
        {
            Id = 1,
            IsActive = true,
            Email = "active@example.com"
        };

        // Act
        var target = new MapWhenNegationTarget(entity);

        // Assert
        target.Email.ShouldBeNull();
    }

    [Fact]
    public void Constructor_ShouldMapWhenNotEqualConditionIsTrue()
    {
        // Arrange
        var completedTime = new DateTime(2024, 1, 15);
        var entity = new MapWhenTestEntity
        {
            Id = 1,
            Status = MapWhenOrderStatus.Completed,
            CompletedAt = completedTime
        };

        // Act
        var target = new MapWhenNotEqualTarget(entity);

        // Assert
        target.CompletedAt.ShouldBe(completedTime);
    }

    [Fact]
    public void Constructor_ShouldNotMapWhenNotEqualConditionIsFalse()
    {
        // Arrange
        var entity = new MapWhenTestEntity
        {
            Id = 1,
            Status = MapWhenOrderStatus.Cancelled,
            CompletedAt = DateTime.Now
        };

        // Act
        var target = new MapWhenNotEqualTarget(entity);

        // Assert
        target.CompletedAt.ShouldBeNull();
    }
}
