using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

/// <summary>
///     Tests for GitHub issue #251 - Nullable issues with object properties.
///     When a record target has a single reference type property like List&lt;string&gt;,
///     the generated constructors should not cause ambiguity with the record's
///     compiler-generated copy constructor.
/// </summary>
public class RecordListPropertyTests
{
    [Fact]
    public void RecordWithListDefault_ShouldCompileAndConstruct()
    {
        // Arrange
        var source = new ModelWithListProperty
        {
            Tags = ["tag1", "tag2"]
        };

        // Act - Should not throw ambiguous constructor error
        var dto = new RecordWithListDefault(source);

        // Assert
        dto.Tags.ShouldBe(new[] { "tag1", "tag2" });
    }

    [Fact]
    public void RecordWithListDefault_ParameterlessConstructor_ShouldWork()
    {
        // Act - Should be able to call parameterless constructor without ambiguity
        var dto = new RecordWithListDefault();

        // Assert - Tags should be default(List<string>)! which is null
        // The null-forgiving operator ensures no null reference issues at compile time
        dto.Tags.ShouldBeNull();
    }

    [Fact]
    public void RecordWithListNoParameterless_ShouldConstructFromSource()
    {
        // Arrange
        var source = new ModelWithListProperty
        {
            Tags = ["test"]
        };

        // Act
        var dto = new RecordWithListNoParameterless(source);

        // Assert
        dto.Tags.ShouldHaveSingleItem().ShouldBe("test");
    }

    [Fact]
    public void RecordWithListNoProjection_ShouldConstructFromSource()
    {
        // Arrange
        var source = new ModelWithListProperty
        {
            Tags = ["a", "b", "c"]
        };

        // Act
        var dto = new RecordWithListNoProjection(source);

        // Assert
        dto.Tags.Count().ShouldBe(3);
    }

    [Fact]
    public void RecordWithMultipleProperties_ShouldWork()
    {
        // Arrange
        var source = new ModelWithMultipleProperties
        {
            Name  = "Test",
            Tags  = ["tag1", "tag2"],
            Count = 42
        };

        // Act
        var dto = new RecordWithMultipleProperties(source);

        // Assert
        dto.Name.ShouldBe("Test");
        dto.Tags.ShouldBe(new[] { "tag1", "tag2" });
        dto.Count.ShouldBe(42);
    }

    [Fact]
    public void RecordWithMultipleProperties_ParameterlessConstructor_ShouldWork()
    {
        // Act
        var dto = new RecordWithMultipleProperties();

        // Assert - string gets string.Empty, List gets default, int gets 0
        dto.Name.ShouldBeEmpty();
        dto.Tags.ShouldBeNull();
        dto.Count.ShouldBe(0);
    }

    [Fact]
    public void RecordWithNullableList_ShouldWork()
    {
        // Arrange
        var source = new ModelWithNullableList
        {
            Tags = ["nullable"]
        };

        // Act
        var dto = new RecordWithNullableList(source);

        // Assert
        dto.Tags.ShouldHaveSingleItem().ShouldBe("nullable");
    }

    [Fact]
    public void RecordWithNullableList_NullValue_ShouldWork()
    {
        // Arrange
        var source = new ModelWithNullableList
        {
            Tags = null
        };

        // Act
        var dto = new RecordWithNullableList(source);

        // Assert
        dto.Tags.ShouldBeNull();
    }

    [Fact]
    public void RecordWithListDefault_WithExpression_ShouldWork()
    {
        // Arrange
        var source = new ModelWithListProperty
        {
            Tags = ["original"]
        };
        var dto = new RecordWithListDefault(source);

        // Act - Records should support 'with' expressions
        var modified = dto with { Tags = ["modified"]
        };

        // Assert
        modified.Tags.ShouldHaveSingleItem().ShouldBe("modified");
        dto.Tags.ShouldHaveSingleItem().ShouldBe("original");
    }
}
