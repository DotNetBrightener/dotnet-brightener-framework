namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

// Test entities
public class AccessibilityTestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Internal target type - should generate with internal accessibility
[MappingTarget<AccessibilityTestEntity>( GenerateToSource = true)]
internal partial class InternalTarget;

// Public target type - should generate with public accessibility
[MappingTarget<AccessibilityTestEntity>( GenerateToSource = true)]
public partial class PublicTarget;

public class AccessibilityTests
{
    [Fact]
    public void InternalTarget_ShouldCompileAndWork()
    {
        // Arrange
        var entity = new AccessibilityTestEntity
        {
            Id = 1,
            Name = "Test"
        };

        // Act
        var target = new InternalTarget(entity);

        // Assert
        target.ShouldNotBeNull();
        target.Id.ShouldBe(1);
        target.Name.ShouldBe("Test");
    }

    [Fact]
    public void PublicTarget_ShouldCompileAndWork()
    {
        // Arrange
        var entity = new AccessibilityTestEntity
        {
            Id = 2,
            Name = "Public Test"
        };

        // Act
        var target = new PublicTarget(entity);

        // Assert
        target.ShouldNotBeNull();
        target.Id.ShouldBe(2);
        target.Name.ShouldBe("Public Test");
    }

    [Fact]
    public void InternalTarget_ShouldHaveInternalAccessibility()
    {
        // Arrange & Act
        var targetType = typeof(InternalTarget);

        // Assert - Type should not be public (internal types are not public)
        targetType.IsPublic.ShouldBeFalse("InternalTarget should have internal accessibility");
        targetType.IsNotPublic.ShouldBeTrue("InternalTarget should have internal accessibility");
    }

    [Fact]
    public void PublicTarget_ShouldHavePublicAccessibility()
    {
        // Arrange & Act
        var targetType = typeof(PublicTarget);

        // Assert
        targetType.IsPublic.ShouldBeTrue("PublicTarget should have public accessibility");
    }

    [Fact]
    public void InternalTarget_ToSource_ShouldWork()
    {
        // Arrange
        var target = new InternalTarget
        {
            Id = 3,
            Name = "ToSource Test"
        };

        // Act
        var entity = target.ToSource();

        // Assert
        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(3);
        entity.Name.ShouldBe("ToSource Test");
    }

    [Fact]
    public void InternalTarget_Projection_ShouldWork()
    {
        // Arrange
        var entities = new[]
        {
            new AccessibilityTestEntity { Id = 1, Name = "First" },
            new AccessibilityTestEntity { Id = 2, Name = "Second" }
        }.AsQueryable();

        // Act
        var targets = entities.Select(InternalTarget.Projection).ToList();

        // Assert
        targets.Count().ShouldBe(2);
        targets[0].Id.ShouldBe(1);
        targets[1].Name.ShouldBe("Second");
    }
}
