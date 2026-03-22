using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Features;

public class NullableHandlingTests
{
    [Fact]
    public void ToTarget_ShouldPreserveNullableStringTypes_WhenMappingToTarget()
    {
        // Arrange
        var testEntity = new NullableTestEntity
        {
            Test1 = true,
            Test2 = false,
            Test3 = "Non-nullable string",
            Test4 = null
        };

        // Act
        var dto = testEntity.ToTarget<NullableTestEntity, NullableTestDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.Test1.ShouldBe(true);
        dto.Test2.ShouldBe(false);
        dto.Test3.ShouldBe("Non-nullable string");
        dto.Test4.ShouldBeNull();
    }

    [Fact]
    public void NullableTestDto_ShouldHaveCorrectPropertyTypes()
    {
        // Arrange & Act
        var dtoType = typeof(NullableTestDto);
        
        // Assert
        var test1Property = dtoType.GetProperty("Test1");
        var test2Property = dtoType.GetProperty("Test2");
        var test3Property = dtoType.GetProperty("Test3");
        var test4Property = dtoType.GetProperty("Test4");

        test1Property.ShouldNotBeNull();
        test1Property!.PropertyType.ShouldBe(typeof(bool));

        test2Property.ShouldNotBeNull();
        test2Property!.PropertyType.ShouldBe(typeof(bool?));

        test3Property.ShouldNotBeNull();
        test3Property!.PropertyType.ShouldBe(typeof(string));

        test4Property.ShouldNotBeNull();
        test4Property!.PropertyType.ShouldBe(typeof(string));
        
        // In C# 8+ with nullable reference types enabled, 
        // we need to check the nullable annotation context
        var nullabilityContext = new System.Reflection.NullabilityInfoContext();
        var test4NullabilityInfo = nullabilityContext.Create(test4Property);
        
        // Test4 should allow null values
        test4NullabilityInfo.ReadState.ShouldBe(System.Reflection.NullabilityState.Nullable);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Some value")]
    public void ToTarget_ShouldHandleNullableStringAssignment_Correctly(string? testValue)
    {
        // Arrange
        var testEntity = new NullableTestEntity
        {
            Test1 = false,
            Test2 = null,
            Test3 = "Always has value",
            Test4 = testValue
        };

        // Act
        var dto = testEntity.ToTarget<NullableTestEntity, NullableTestDto>();

        // Assert
        dto.Test4.ShouldBe(testValue);
    }
}
