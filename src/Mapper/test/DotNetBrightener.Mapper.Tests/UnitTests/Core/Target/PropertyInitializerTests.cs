using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

public class PropertyInitializerTests
{
    [Fact]
    public void Target_ShouldPreserveInitializer_ForNonNullableReferenceTypeProperty()
    {
        // Arrange & Act
        var dto = new UserModelDto();

        // Assert
        dto.Settings.ShouldNotBeNull("Settings should be initialized with default value from source");
    }

    [Fact]
    public void Target_ShouldMapFromSource_WithInitializedProperties()
    {
        // Arrange
        var source = new UserModel
        {
            Id = 1,
            Name = "Test User",
            Settings = new UserSettings
            {
                NotificationsEnabled = false,
                Theme = "dark",
                Language = "de"
            }
        };

        // Act
        var dto = new UserModelDto(source);

        // Assert
        dto.Id.ShouldBe(1);
        dto.Name.ShouldBe("Test User");
        dto.Settings.ShouldNotBeNull();
        dto.Settings.NotificationsEnabled.ShouldBeFalse();
        dto.Settings.Theme.ShouldBe("dark");
        dto.Settings.Language.ShouldBe("de");
    }

    [Fact]
    public void Target_ShouldPreserveStringEmptyInitializer()
    {
        // Arrange & Act
        var dto = new UserModelDto();

        // Assert - Name should be initialized to string.Empty from the source type
        dto.Name.ShouldBeEmpty("Name should be initialized to string.Empty");
    }

    [Fact]
    public void Target_ShouldPreserveListInitializer_ForInitOnlyProperties()
    {
        // Arrange & Act
        var dto = new InitOnlyWithInitializersDto();

        // Assert - Tags should be initialized to new List<string>()
        dto.Tags.ShouldNotBeNull("Tags should be initialized from source");
        dto.Tags.ShouldBeEmpty();
    }

    [Fact]
    public void Target_ShouldPreserveGuidInitializer_ForIdProperty()
    {
        // Arrange & Act
        var dto = new InitOnlyWithInitializersDto();

        // Assert
        dto.Id.ShouldNotBeNullOrEmpty();
        Guid.TryParse(dto.Id, out _).ShouldBeTrue("Id should be a valid GUID format");
    }

    [Fact]
    public void Target_ShouldPreserveDateTimeUtcNowInitializer()
    {
        // Arrange & Act
        var beforeCreation = DateTime.UtcNow;
        var dto = new InitOnlyWithInitializersDto();
        var afterCreation = DateTime.UtcNow;

        // Assert
        dto.CreatedAt.ShouldBeGreaterThanOrEqualTo(beforeCreation.AddSeconds(-1));
        dto.CreatedAt.ShouldBeLessThanOrEqualTo(afterCreation.AddSeconds(1));
    }

    [Fact]
    public void Target_ShouldCopyFromSource_OverridingDefaultInitializer()
    {
        // Arrange
        var customId = "custom-id-123";
        var customDate = new DateTime(2020, 1, 1);
        var source = new InitOnlyWithInitializers
        {
            Id        = customId,
            Name      = "Custom Name",
            Tags      = ["tag1", "tag2"],
            CreatedAt = customDate
        };

        // Act
        var dto = new InitOnlyWithInitializersDto(source);

        // Assert
        dto.Id.ShouldBe(customId);
        dto.Name.ShouldBe("Custom Name");
        dto.Tags.ShouldBe(new[] { "tag1", "tag2" });
        dto.CreatedAt.ShouldBe(customDate);
    }
}
