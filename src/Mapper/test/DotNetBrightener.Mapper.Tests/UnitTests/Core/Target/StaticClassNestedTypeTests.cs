using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

public class StaticClassNestedTypeTests
{
    [Fact]
    public void Target_ShouldGenerateCorrectly_WhenSourceTypeIsNestedInStaticClass()
    {
        // Arrange
        var bar = new Foo.Bar
        {
            Name = "Test",
            Value = 42
        };

        // Act
        var dto = new BarDto(bar);

        // Assert
        dto.ShouldNotBeNull();
        dto.Name.ShouldBe("Test");
        dto.Value.ShouldBe(42);
    }

    [Fact]
    public void Target_ShouldMap_WhenSourceTypeIsNestedInStaticClass()
    {
        // Arrange
        var bar = new Foo.Bar
        {
            Name = "Test",
            Value = 42
        };

        // Act
        var dto = bar.ToTarget<Foo.Bar, BarDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.Name.ShouldBe("Test");
        dto.Value.ShouldBe(42);
    }

    [Fact]
    public void Target_ShouldGenerateCorrectly_WhenSourceHasNestedClassProperty()
    {
        // Arrange - issue #272: nested class property should generate using static, not using
        var bar = new Foo.Bar
        {
            Name = "Test",
            Value = 42,
            Arr1 = new Foo.Bar.Arr { Length = 10 }
        };

        // Act
        var dto = new BarDto(bar);

        // Assert
        dto.ShouldNotBeNull();
        dto.Name.ShouldBe("Test");
        dto.Value.ShouldBe(42);
        dto.Arr1.ShouldNotBeNull();
        dto.Arr1!.Length.ShouldBe(10);
    }

    [Fact]
    public void Target_ShouldHandleNullNestedClassProperty()
    {
        // Arrange
        var bar = new Foo.Bar
        {
            Name = "Test",
            Value = 42,
            Arr1 = null
        };

        // Act
        var dto = new BarDto(bar);

        // Assert
        dto.ShouldNotBeNull();
        dto.Arr1.ShouldBeNull();
    }
}
