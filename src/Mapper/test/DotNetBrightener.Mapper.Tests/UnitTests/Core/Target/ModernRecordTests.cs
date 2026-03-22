using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Tests.TestModels;
using DotNetBrightener.Mapper.Tests.Utilities;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

public class ModernRecordTests
{
    [Fact]
    public void ToTarget_ShouldMapModernRecord_WithRequiredProperties()
    {
        // Arrange
        var modernUser = TestDataFactory.CreateModernUser("Modern", "User");

        // Act
        var dto = modernUser.ToTarget<ModernUser, ModernUserDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(modernUser.Id);
        dto.FirstName.ShouldBe("Modern");
        dto.LastName.ShouldBe("User");
        dto.Email.ShouldBe(modernUser.Email);
        dto.CreatedAt.ShouldBe(modernUser.CreatedAt);
    }

    [Fact]
    public void ToTarget_ShouldExcludeSpecifiedProperties_FromModernRecord()
    {
        // Arrange
        var modernUser = TestDataFactory.CreateModernUser();

        // Act
        var dto = modernUser.ToTarget<ModernUser, ModernUserDto>();

        // Assert
        var dtoType = dto.GetType();
        dtoType.GetProperty("PasswordHash").ShouldBeNull("PasswordHash should be excluded");
        dtoType.GetProperty("Bio").ShouldBeNull("Bio should be excluded");
    }

    [Fact]
    public void ToTarget_ShouldHandleNullableProperties_InModernRecord()
    {
        // Arrange
        var modernUser = new ModernUser
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = "Test",
            LastName = "User",
            Email = null, // Nullable property
            CreatedAt = DateTime.UtcNow,
            Bio = "Should be excluded",
            PasswordHash = "Should be excluded"
        };

        // Act
        var dto = modernUser.ToTarget<ModernUser, ModernUserDto>();

        // Assert
        dto.Email.ShouldBeNull();
        dto.FirstName.ShouldBe("Test");
        dto.LastName.ShouldBe("User");
    }

    [Fact]
    public void ToTarget_ModernRecordDto_ShouldSupportRecordFeatures()
    {
        // Arrange
        var modernUser = TestDataFactory.CreateModernUser("Record", "Features");

        // Act
        var dto = modernUser.ToTarget<ModernUser, ModernUserDto>();
        var dto2 = modernUser.ToTarget<ModernUser, ModernUserDto>();

        // Assert
        dto.Equals(dto2).ShouldBeTrue("Records should have value equality");
        dto.GetHashCode().ShouldBe(dto2.GetHashCode(), "Equal records should have same hash code");
    }

    [Fact]
    public void ToTarget_ModernRecordDto_ShouldSupportWithExpressions()
    {
        // Arrange
        var modernUser = TestDataFactory.CreateModernUser("With", "Expression");
        var dto = modernUser.ToTarget<ModernUser, ModernUserDto>();

        // Act
        var modifiedDto = dto with { FirstName = "Modified" };

        // Assert
        modifiedDto.FirstName.ShouldBe("Modified");
        modifiedDto.LastName.ShouldBe("Expression"); // Other properties unchanged
        dto.FirstName.ShouldBe("With"); // Original unchanged
    }

    [Fact]
    public void ToTarget_ModernRecordDto_ShouldHandleInitOnlyProperties()
    {
        // Arrange
        var modernUser = new ModernUser
        {
            Id = "init-only-test",
            FirstName = "Init",
            LastName = "Only",
            CreatedAt = DateTime.UtcNow // This is init-only in the source
        };

        // Act
        var dto = modernUser.ToTarget<ModernUser, ModernUserDto>();

        // Assert
        dto.Id.ShouldBe("init-only-test");
        dto.CreatedAt.ShouldBe(modernUser.CreatedAt);
    }

    [Fact]
    public void ToTarget_ShouldMapCustomPropertiesInRecord()
    {
        // Arrange
        var modernUser = TestDataFactory.CreateModernUser("Custom", "Props");

        // Act
        var dto = modernUser.ToTarget<ModernUser, ModernUserDto>();

        // Assert
        // The DTO can have custom properties that don't exist in the source
        dto.FullName.ShouldBe(string.Empty); // Default value for custom property
        dto.DisplayName.ShouldBe(string.Empty); // Default value for custom property
        
        // But source properties should still be mapped
        dto.FirstName.ShouldBe("Custom");
        dto.LastName.ShouldBe("Props");
    }

    [Fact]
    public void ToTarget_ShouldHandleGuidIds_InModernRecords()
    {
        // Arrange
        var guidId = Guid.NewGuid().ToString();
        var modernUser = new ModernUser
        {
            Id = guidId,
            FirstName = "Guid",
            LastName = "Test",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var dto = modernUser.ToTarget<ModernUser, ModernUserDto>();

        // Assert
        dto.Id.ShouldBe(guidId);
        Guid.TryParse(dto.Id, out _).ShouldBeTrue("ID should be a valid GUID string");
    }

    [Fact]
    public void ToTarget_ModernRecord_ShouldPreservePropertyCasing()
    {
        // Arrange
        var modernUser = TestDataFactory.CreateModernUser("Case", "Sensitive");

        // Act
        var dto = modernUser.ToTarget<ModernUser, ModernUserDto>();

        // Assert
        dto.FirstName.ShouldBe("Case"); // Exact case preserved
        dto.LastName.ShouldBe("Sensitive"); // Exact case preserved
        
        // Property names should match exactly (case-sensitive)
        var dtoType = dto.GetType();
        dtoType.GetProperty("FirstName").ShouldNotBeNull();
        dtoType.GetProperty("firstname").ShouldBeNull(); // lowercase should not exist
    }
}
