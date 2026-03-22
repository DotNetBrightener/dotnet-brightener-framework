namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

// Test entities
public class InheritedMemberEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int State { get; set; }
    public string LocalName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

// Base class for targets
public abstract class BaseMemberTarget
{
    public int Id { get; set; }
    public int State { get; set; }
}

// Target that inherits from base class - should not generate Id and State again
[MappingTarget<InheritedMemberEntity>( exclude: ["LocalName"])]
public partial class InheritedMemberTarget : BaseMemberTarget
{
}

// Another base class scenario
public abstract class BaseWithName
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

[MappingTarget<InheritedMemberEntity>( Include = ["Id", "Name", "Description"])]
public partial class InheritedIncludeTarget : BaseWithName
{
}

public class InheritedMemberTests
{
    [Fact]
    public void Constructor_ShouldNotGenerateDuplicateProperties()
    {
        // Arrange
        var entity = new InheritedMemberEntity
        {
            Id = 1,
            Name = "Test",
            State = 42,
            LocalName = "Local",
            Description = "Description"
        };

        // Act
        var target = new InheritedMemberTarget(entity);

        // Assert
        target.Id.ShouldBe(1);
        target.State.ShouldBe(42);
        target.Name.ShouldBe("Test");
        target.Description.ShouldBe("Description");
    }

    [Fact]
    public void TargetType_ShouldNotHaveDuplicateProperties()
    {
        // Verify that the target type doesn't have duplicate properties
        var targetType = typeof(InheritedMemberTarget);
        var declaredProperties = targetType.GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        // Id and State should NOT be in declared properties since they're inherited
        var propertyNames = declaredProperties.Select(p => p.Name).ToList();
        propertyNames.ShouldNotContain("Id");
        propertyNames.ShouldNotContain("State");

        // Name and Description should be in declared properties (not in base)
        propertyNames.ShouldContain("Name");
        propertyNames.ShouldContain("Description");
    }

    [Fact]
    public void IncludeMode_ShouldNotGenerateDuplicateProperties()
    {
        // Arrange
        var entity = new InheritedMemberEntity
        {
            Id = 2,
            Name = "Test2",
            Description = "Desc2"
        };

        // Act
        var target = new InheritedIncludeTarget(entity);

        // Assert
        target.Id.ShouldBe(2);
        target.Name.ShouldBe("Test2");
        target.Description.ShouldBe("Desc2");

        // Verify no duplicate properties
        var targetType = typeof(InheritedIncludeTarget);
        var declaredProperties = targetType.GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var propertyNames = declaredProperties.Select(p => p.Name).ToList();

        // Id and Name should NOT be declared since they're inherited
        propertyNames.ShouldNotContain("Id");
        propertyNames.ShouldNotContain("Name");

        // Description should be declared
        propertyNames.ShouldContain("Description");
    }

    [Fact]
    public void Projection_ShouldWorkWithInheritedProperties()
    {
        // Arrange
        var entities = new[]
        {
            new InheritedMemberEntity { Id = 1, Name = "Test1", State = 10, Description = "Desc1" },
            new InheritedMemberEntity { Id = 2, Name = "Test2", State = 20, Description = "Desc2" }
        }.AsQueryable();

        // Act
        var targets = entities.Select(InheritedMemberTarget.Projection).ToList();

        // Assert
        targets.Count().ShouldBe(2);
        targets[0].Id.ShouldBe(1);
        targets[0].State.ShouldBe(10);
        targets[0].Name.ShouldBe("Test1");
        targets[1].Id.ShouldBe(2);
        targets[1].State.ShouldBe(20);
        targets[1].Name.ShouldBe("Test2");
    }
}
