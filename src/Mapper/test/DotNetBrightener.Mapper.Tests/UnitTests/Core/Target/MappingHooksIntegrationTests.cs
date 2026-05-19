using DotNetBrightener.Mapper.Mapping.Configurations;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

// Test entity for generated hooks
public class GeneratedHooksEntity
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public bool IsActive { get; set; }
}

// BeforeMap hook configuration
public class GeneratedBeforeMapConfig : IBeforeMapConfiguration<GeneratedHooksEntity, GeneratedBeforeMapTarget>
{
    public static void BeforeMap(GeneratedHooksEntity source, GeneratedBeforeMapTarget target)
    {
        target.MappedAt = DateTime.UtcNow;
    }
}

// AfterMap hook configuration
public class GeneratedAfterMapConfig : IAfterMapConfiguration<GeneratedHooksEntity, GeneratedAfterMapTarget>
{
    public static void AfterMap(GeneratedHooksEntity source, GeneratedAfterMapTarget target)
    {
        target.FullName = $"{target.FirstName} {target.LastName}";
    }
}

// Combined hooks configuration
public class GeneratedCombinedConfig : IMapHooksConfiguration<GeneratedHooksEntity, GeneratedCombinedTarget>
{
    public static void BeforeMap(GeneratedHooksEntity source, GeneratedCombinedTarget target)
    {
        target.MappedAt = DateTime.UtcNow;
    }

    public static void AfterMap(GeneratedHooksEntity source, GeneratedCombinedTarget target)
    {
        target.FullName = $"{target.FirstName} {target.LastName}";
    }
}

// Generated target with BeforeMap
[MappingTarget<GeneratedHooksEntity>( BeforeMapConfiguration = typeof(GeneratedBeforeMapConfig))]
public partial class GeneratedBeforeMapTarget
{
    public DateTime MappedAt { get; set; }
}

// Generated target with AfterMap
[MappingTarget<GeneratedHooksEntity>( AfterMapConfiguration = typeof(GeneratedAfterMapConfig))]
public partial class GeneratedAfterMapTarget
{
    public string FullName { get; set; } = string.Empty;
}

// Generated target with both hooks
[MappingTarget<GeneratedHooksEntity>(
    BeforeMapConfiguration = typeof(GeneratedCombinedConfig),
    AfterMapConfiguration = typeof(GeneratedCombinedConfig))]
public partial class GeneratedCombinedTarget
{
    public DateTime MappedAt { get; set; }
    public string FullName { get; set; } = string.Empty;
}

/// <summary>
///     Integration tests for Before/After mapping hooks with generated targets.
/// </summary>
public class MappingHooksIntegrationTests
{
    [Fact]
    public void GeneratedTarget_WithBeforeMap_ShouldSetMappedAt()
    {
        // Arrange
        var entity = new GeneratedHooksEntity
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateTime.Today.AddYears(-30),
            IsActive = true
        };
        var beforeCall = DateTime.UtcNow;

        // Act
        var target = new GeneratedBeforeMapTarget(entity);
        var afterCall = DateTime.UtcNow;

        // Assert
        target.Id.ShouldBe(1);
        target.FirstName.ShouldBe("John");
        target.LastName.ShouldBe("Doe");
        target.MappedAt.ShouldBeGreaterThanOrEqualTo(beforeCall);
        target.MappedAt.ShouldBeLessThanOrEqualTo(afterCall);
    }

    [Fact]
    public void GeneratedTarget_WithAfterMap_ShouldComputeFullName()
    {
        // Arrange
        var entity = new GeneratedHooksEntity
        {
            Id = 2,
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = DateTime.Today.AddYears(-25),
            IsActive = true
        };

        // Act
        var target = new GeneratedAfterMapTarget(entity);

        // Assert
        target.Id.ShouldBe(2);
        target.FirstName.ShouldBe("Jane");
        target.LastName.ShouldBe("Smith");
        target.FullName.ShouldBe("Jane Smith");
    }

    [Fact]
    public void GeneratedTarget_WithCombinedHooks_ShouldCallBothBeforeAndAfter()
    {
        // Arrange
        var entity = new GeneratedHooksEntity
        {
            Id = 3,
            FirstName = "Bob",
            LastName = "Johnson",
            DateOfBirth = DateTime.Today.AddYears(-40),
            IsActive = false
        };
        var beforeCall = DateTime.UtcNow;

        // Act
        var target = new GeneratedCombinedTarget(entity);
        var afterCall = DateTime.UtcNow;

        // Assert
        target.Id.ShouldBe(3);
        target.FirstName.ShouldBe("Bob");
        target.LastName.ShouldBe("Johnson");
        target.MappedAt.ShouldBeGreaterThanOrEqualTo(beforeCall);
        target.MappedAt.ShouldBeLessThanOrEqualTo(afterCall);
        target.FullName.ShouldBe("Bob Johnson");
    }

    [Fact]
    public void GeneratedTarget_WithBeforeMap_ShouldWorkWithFromSource()
    {
        // Arrange
        var entity = new GeneratedHooksEntity
        {
            Id = 4,
            FirstName = "Alice",
            LastName = "Brown",
            DateOfBirth = DateTime.Today.AddYears(-28),
            IsActive = true
        };
        var beforeCall = DateTime.UtcNow;

        // Act
        var target = GeneratedBeforeMapTarget.FromSource(entity);
        var afterCall = DateTime.UtcNow;

        // Assert
        target.FirstName.ShouldBe("Alice");
        target.MappedAt.ShouldBeGreaterThanOrEqualTo(beforeCall);
        target.MappedAt.ShouldBeLessThanOrEqualTo(afterCall);
    }

    [Fact]
    public void GeneratedTarget_WithAfterMap_ShouldWorkWithProjection()
    {
        // Arrange
        var entities = new[]
        {
            new GeneratedHooksEntity { Id = 1, FirstName = "John", LastName = "Doe" },
            new GeneratedHooksEntity { Id = 2, FirstName = "Jane", LastName = "Smith" }
        }.AsQueryable();

        // Act - Projection doesn't call hooks (they're runtime-only)
        var targets = entities.Select(GeneratedAfterMapTarget.Projection).ToList();

        // Assert - Properties are mapped but FullName is NOT computed (hooks don't run in projections)
        targets.Count().ShouldBe(2);
        targets[0].FirstName.ShouldBe("John");
        targets[1].FirstName.ShouldBe("Jane");
    }
}
