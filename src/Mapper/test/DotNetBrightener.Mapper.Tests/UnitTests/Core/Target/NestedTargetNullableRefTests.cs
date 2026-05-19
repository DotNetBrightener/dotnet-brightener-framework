namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

/// <summary>
///     Tests for GitHub issue #261: nested target + nullable ref compilation error
///     When preserveReferences is enabled, the Select can return null for circular references,
///     which causes the type to be inferred as List&lt;T?&gt; even after Where(x => x != null).
/// </summary>
public partial class NestedTargetNullableRefTests
{
    // Domain classes
    public sealed class DomainObject
    {
        public List<DomainObjectItem> Items { get; set; } = [];
    }

    public sealed class DomainObjectItem
    {
        public string? Value { get; set; }
    }

    // DTOs with nested targets - with PreserveReferences enabled to trigger the nullable issue
    [MappingTarget<DomainObject>( MaxDepth = 2, PreserveReferences = true, NestedTargetTypes = [typeof(DomainObjectItemDto)])]
    public sealed partial class DomainObjectDto;

    [MappingTarget<DomainObjectItem>()]
    public sealed partial class DomainObjectItemDto;

    [Fact]
    public void NestedTargetCollection_WithNullableRefEnabled_ShouldCompile()
    {
        // Arrange
        var domain = new DomainObject
        {
            Items =
            [
                new DomainObjectItem { Value = "Item 1" },
                new DomainObjectItem { Value = "Item 2" },
                new DomainObjectItem { Value = null }
            ]
        };

        // Act
        var dto = new DomainObjectDto(domain);

        // Assert
        Assert.NotNull(dto.Items);
        Assert.Equal(3, dto.Items.Count);
        Assert.Equal("Item 1", dto.Items[0].Value);
        Assert.Equal("Item 2", dto.Items[1].Value);
        Assert.Null(dto.Items[2].Value);
    }

    [Fact]
    public void NestedTargetCollection_WithCircularReference_ShouldHandleCorrectly()
    {
        // Arrange - This test ensures circular reference detection works correctly
        var domain = new DomainObject
        {
            Items =
            [
                new DomainObjectItem { Value = "Item 1" }
            ]
        };

        // Act
        var dto = new DomainObjectDto(domain);

        // Assert
        Assert.NotNull(dto.Items);
        Assert.Single(dto.Items);
        Assert.Equal("Item 1", dto.Items[0].Value);
    }
}
