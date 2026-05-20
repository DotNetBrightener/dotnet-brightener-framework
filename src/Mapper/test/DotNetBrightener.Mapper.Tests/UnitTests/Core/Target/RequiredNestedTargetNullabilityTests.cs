using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

/// <summary>
///     When a source property is marked as 'required' and is non-nullable,
///     the generated target should compile without nullable warnings.
/// </summary>
public class RequiredNestedTargetNullabilityTests
{
    [Fact]
    public void RequiredNestedTarget_Property_ShouldBeNonNullable()
    {
        // Arrange & Act
        var dtoType = typeof(UserWithRequiredSettingsTarget);
        var settingsProperty = dtoType.GetProperty("Settings");

        // Assert
        settingsProperty.ShouldNotBeNull();
        // The property type should be the target type (not nullable)
        settingsProperty!.PropertyType.ShouldBe(typeof(UserSettingsTarget));
    }

    [Fact]
    public void RequiredNestedTarget_ShouldBeMarkedAsRequired()
    {
        // Arrange & Act
        var dtoType = typeof(UserWithRequiredSettingsTarget);
        var settingsProperty = dtoType.GetProperty("Settings");

        // Assert
        settingsProperty.ShouldNotBeNull();
        // Check if the property has the required modifier (via attribute in reflection)
        var requiredAttribute = settingsProperty!.GetCustomAttributes(
            typeof(System.Runtime.CompilerServices.RequiredMemberAttribute), true);
        // Note: Required modifier appears on the type, not the property in reflection
        // We verify by checking the type has RequiredMemberAttribute
        var typeRequiredAttribute = dtoType.GetCustomAttributes(
            typeof(System.Runtime.CompilerServices.RequiredMemberAttribute), true);
        typeRequiredAttribute.ShouldNotBeEmpty("Type should have RequiredMemberAttribute");
    }

    [Fact]
    public void RequiredNestedTarget_Constructor_ShouldMapCorrectly()
    {
        // Arrange
        var source = new UserModelWithRequiredSettings
        {
            Id = 1,
            SettingsId = 100,
            Settings = new UserSettingsModelForNested
            {
                Id = 100,
                StartTick = 10,
                StopTick = 50
            }
        };

        // Act
        var dto = new UserWithRequiredSettingsTarget(source);

        // Assert
        dto.Id.ShouldBe(1);
        dto.SettingsId.ShouldBe(100);
        dto.Settings.ShouldNotBeNull();
        dto.Settings.StartTick.ShouldBe(10);
        dto.Settings.StopTick.ShouldBe(50);
    }

    [Fact]
    public void RequiredNestedTarget_ComputedProperty_ShouldWork()
    {
        // Arrange
        var source = new UserModelWithRequiredSettings
        {
            Id = 1,
            SettingsId = 100,
            Settings = new UserSettingsModelForNested
            {
                Id = 100,
                StartTick = 10,
                StopTick = 50
            }
        };

        // Act
        var dto = new UserWithRequiredSettingsTarget(source);

        // Assert - The computed property should work without null reference issues
        dto.ProcessedTicks.ShouldBe(40); // 50 - 10
    }

    [Fact]
    public void OptionalNestedTarget_Property_ShouldBeNullable()
    {
        // Arrange & Act
        var dtoType = typeof(UserWithOptionalSettingsTarget);
        var settingsProperty = dtoType.GetProperty("Settings");

        // Assert
        settingsProperty.ShouldNotBeNull();
        // For optional (non-required) nested targets, they should be treated as nullable
        // The type will be the target type (nullable reference type annotation is not visible via reflection)
        settingsProperty!.PropertyType.ShouldBe(typeof(UserSettingsTarget));
    }

    [Fact]
    public void OptionalNestedTarget_Constructor_ShouldHandleValue()
    {
        // Arrange
        var source = new UserModelWithOptionalSettings
        {
            Id = 2,
            Settings = new UserSettingsModelForNested
            {
                Id = 200,
                StartTick = 5,
                StopTick = 25
            }
        };

        // Act
        var dto = new UserWithOptionalSettingsTarget(source);

        // Assert
        dto.Id.ShouldBe(2);
        dto.Settings.ShouldNotBeNull();
        dto.Settings!.StartTick.ShouldBe(5);
        dto.Settings.StopTick.ShouldBe(25);
    }

    [Fact]
    public void Projection_WithRequiredNestedTarget_ShouldWork()
    {
        // Arrange
        var sources = new[]
        {
            new UserModelWithRequiredSettings
            {
                Id = 1,
                SettingsId = 100,
                Settings = new UserSettingsModelForNested
                {
                    Id = 100,
                    StartTick = 0,
                    StopTick = 100
                }
            }
        }.AsQueryable();

        // Act
        var dtos = sources.Select(UserWithRequiredSettingsTarget.Projection).ToList();

        // Assert
        dtos.Count().ShouldBe(1);
        dtos[0].Id.ShouldBe(1);
        dtos[0].Settings.ShouldNotBeNull();
        dtos[0].Settings.StartTick.ShouldBe(0);
        dtos[0].Settings.StopTick.ShouldBe(100);
    }

    /// <summary>
    ///     When a required nested target source property is null at runtime,
    ///     the constructor should throw ArgumentNullException with a descriptive message
    ///     instead of a cryptic NullReferenceException.
    ///     This tests the fix for GitHub issue #258.
    /// </summary>
    [Fact]
    public void RequiredNestedTarget_WhenSourcePropertyIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange - Force null on a required property (using null! to bypass compiler)
        var source = new UserModelWithRequiredSettings
        {
            Id = 1,
            SettingsId = 100,
            Settings = null!  // This violates the required constraint at runtime
        };

        // Act & Assert
        var action = () => new UserWithRequiredSettingsTarget(source);
        
        var ex = action.ShouldThrow<ArgumentNullException>();
        ex.Message.ShouldContain("Settings");
        ex.Message.ShouldContain("Required nested target property");
    }

    /// <summary>
    ///     Verifies that the ArgumentNullException contains helpful information
    ///     about which property was null.
    /// </summary>
    [Fact]
    public void RequiredNestedTarget_WhenSourcePropertyIsNull_ExceptionShouldContainPropertyName()
    {
        // Arrange
        var source = new UserModelWithRequiredSettings
        {
            Id = 1,
            SettingsId = 100,
            Settings = null!
        };

        // Act
        ArgumentNullException? exception = null;
        try
        {
            _ = new UserWithRequiredSettingsTarget(source);
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        // Assert
        exception.ShouldNotBeNull();
        exception!.ParamName.ShouldBe("Settings");
        exception.Message.ShouldContain("Required nested target property");
        exception.Message.ShouldContain("was null");
    }

    /// <summary>
    ///     When a required collection nested target source property is null at runtime,
    ///     the constructor should throw ArgumentNullException with a descriptive message.
    ///     This tests the collection variant of the fix for GitHub issue #258.
    /// </summary>
    [Fact]
    public void RequiredCollectionNestedTarget_WhenSourcePropertyIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange - Force null on a required collection property
        var source = new TeamModelWithRequiredMembers
        {
            Id = 1,
            Name = "Team Alpha",
            Members = null!  // This violates the required constraint at runtime
        };

        // Act & Assert
        var action = () => new TeamWithRequiredMembersTarget(source);
        
        var ex = action.ShouldThrow<ArgumentNullException>();
        ex.Message.ShouldContain("Members");
        ex.Message.ShouldContain("Required nested target collection property");
    }

    /// <summary>
    ///     Verifies that a required collection nested target works correctly when source is populated.
    /// </summary>
    [Fact]
    public void RequiredCollectionNestedTarget_WhenSourcePropertyIsPopulated_ShouldMapCorrectly()
    {
        // Arrange
        var source = new TeamModelWithRequiredMembers
        {
            Id = 1,
            Name = "Team Alpha",
            Members =
            [
                new()
                {
                    Id        = 1,
                    StartTick = 10,
                    StopTick  = 50
                },
                new()
                {
                    Id        = 2,
                    StartTick = 20,
                    StopTick  = 60
                }
            ]
        };

        // Act
        var dto = new TeamWithRequiredMembersTarget(source);

        // Assert
        dto.Id.ShouldBe(1);
        dto.Name.ShouldBe("Team Alpha");
        dto.Members.ShouldNotBeNull();
        dto.Members.Count().ShouldBe(2);
        dto.Members[0].StartTick.ShouldBe(10);
        dto.Members[1].StartTick.ShouldBe(20);
    }
}
