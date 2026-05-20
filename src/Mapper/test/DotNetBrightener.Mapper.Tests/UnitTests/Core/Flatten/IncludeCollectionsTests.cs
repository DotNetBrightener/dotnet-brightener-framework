using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Flatten;

/// <summary>
///     Tests for the IncludeCollections feature in the Flatten attribute (GitHub issue #242).
///     This allows collection properties to be included as-is in flattened objects without
///     flattening their contents.
/// </summary>
public class IncludeCollectionsTests
{
    [Fact]
    public void Flatten_WithIncludeCollections_ShouldIncludeListProperty()
    {
        // Arrange & Act
        var dtoType = typeof(ApiResponseFlatWithCollectionsDto);
        var itemsProperty = dtoType.GetProperty("Items");

        // Assert
        itemsProperty.ShouldNotBeNull("Items collection should be included when IncludeCollections = true");
        itemsProperty!.PropertyType.ShouldBe(typeof(List<ResponseItem>), "Collection type should be preserved");
    }

    [Fact]
    public void Flatten_WithIncludeCollections_ShouldIncludeArrayProperty()
    {
        // Arrange & Act
        var dtoType = typeof(ApiResponseFlatWithCollectionsDto);
        var tagsProperty = dtoType.GetProperty("Tags");

        // Assert
        tagsProperty.ShouldNotBeNull("Tags array should be included when IncludeCollections = true");
        tagsProperty!.PropertyType.ShouldBe(typeof(string[]), "Array type should be preserved");
    }

    [Fact]
    public void Flatten_WithIncludeCollections_ShouldAlsoIncludeScalarProperties()
    {
        // Arrange & Act
        var dtoType = typeof(ApiResponseFlatWithCollectionsDto);

        // Assert - Scalar properties should still be included
        dtoType.GetProperty("Id").ShouldNotBeNull();
        dtoType.GetProperty("Name").ShouldNotBeNull();
        dtoType.GetProperty("MetadataCreatedAt").ShouldNotBeNull("Nested scalar should be flattened");
        dtoType.GetProperty("MetadataVersion").ShouldNotBeNull("Nested scalar should be flattened");
    }

    [Fact]
    public void Flatten_WithoutIncludeCollections_ShouldExcludeListProperty()
    {
        // Arrange & Act
        var dtoType = typeof(ApiResponseFlatWithoutCollectionsDto);
        var itemsProperty = dtoType.GetProperty("Items");

        // Assert
        itemsProperty.ShouldBeNull("Items collection should be excluded when IncludeCollections = false (default)");
    }

    [Fact]
    public void Flatten_WithoutIncludeCollections_ShouldExcludeArrayProperty()
    {
        // Arrange & Act
        var dtoType = typeof(ApiResponseFlatWithoutCollectionsDto);
        var tagsProperty = dtoType.GetProperty("Tags");

        // Assert
        tagsProperty.ShouldBeNull("Tags array should be excluded when IncludeCollections = false (default)");
    }

    [Fact]
    public void Flatten_WithoutIncludeCollections_ShouldStillIncludeScalarProperties()
    {
        // Arrange & Act
        var dtoType = typeof(ApiResponseFlatWithoutCollectionsDto);

        // Assert - Scalar properties should still be included
        dtoType.GetProperty("Id").ShouldNotBeNull();
        dtoType.GetProperty("Name").ShouldNotBeNull();
        dtoType.GetProperty("MetadataCreatedAt").ShouldNotBeNull();
        dtoType.GetProperty("MetadataVersion").ShouldNotBeNull();
    }

    [Fact]
    public void Flatten_WithIncludeCollections_ShouldWorkWithConstructor()
    {
        // Arrange
        var source = new ApiResponse
        {
            Id = 1,
            Name = "Test Response",
            Metadata = new ResponseMetadata
            {
                CreatedAt = new DateTime(2024, 1, 15),
                Version = "1.0"
            },
            Items =
            [
                new ResponseItem
                {
                    ItemId   = 101,
                    ItemName = "Item 1",
                    Price    = 9.99m
                },
                new ResponseItem
                {
                    ItemId   = 102,
                    ItemName = "Item 2",
                    Price    = 19.99m
                }
            ],
            Tags = ["tag1", "tag2", "tag3"]
        };

        // Act
        var dto = new ApiResponseFlatWithCollectionsDto(source);

        // Assert
        dto.Id.ShouldBe(1);
        dto.Name.ShouldBe("Test Response");
        dto.MetadataCreatedAt.ShouldBe(new DateTime(2024, 1, 15));
        dto.MetadataVersion.ShouldBe("1.0");
        dto.Items.Count().ShouldBe(2);
        dto.Items[0].ItemId.ShouldBe(101);
        dto.Items[1].ItemName.ShouldBe("Item 2");
        dto.Tags.ShouldBe(["tag1", "tag2", "tag3"]);
    }

    [Fact]
    public void Flatten_WithIncludeCollections_ShouldPreserveIEnumerable()
    {
        // Arrange & Act
        var dtoType = typeof(EntityWithVariousCollectionsFlatDto);
        var emailsProperty = dtoType.GetProperty("Emails");

        // Assert
        emailsProperty.ShouldNotBeNull();
        emailsProperty!.PropertyType.ShouldBe(typeof(IEnumerable<string>));
    }

    [Fact]
    public void Flatten_WithIncludeCollections_ShouldPreserveICollection()
    {
        // Arrange & Act
        var dtoType = typeof(EntityWithVariousCollectionsFlatDto);
        var numbersProperty = dtoType.GetProperty("Numbers");

        // Assert
        numbersProperty.ShouldNotBeNull();
        numbersProperty!.PropertyType.ShouldBe(typeof(ICollection<int>));
    }

    [Fact]
    public void Flatten_WithIncludeCollections_ShouldPreserveIList()
    {
        // Arrange & Act
        var dtoType = typeof(EntityWithVariousCollectionsFlatDto);
        var datesProperty = dtoType.GetProperty("Dates");

        // Assert
        datesProperty.ShouldNotBeNull();
        datesProperty!.PropertyType.ShouldBe(typeof(IList<DateTime>));
    }

    [Fact]
    public void Flatten_WithIncludeCollections_ShouldPreserveHashSet()
    {
        // Arrange & Act
        var dtoType = typeof(EntityWithVariousCollectionsFlatDto);
        var uniqueValuesProperty = dtoType.GetProperty("UniqueValues");

        // Assert
        uniqueValuesProperty.ShouldNotBeNull();
        uniqueValuesProperty!.PropertyType.ShouldBe(typeof(HashSet<string>));
    }

    [Fact]
    public void Flatten_WithIncludeCollections_ShouldHandleNullCollection()
    {
        // Arrange
        var source = new ApiResponse
        {
            Id = 1,
            Name = "Test",
            Metadata = new ResponseMetadata(),
            Items = null!,
            Tags = null!
        };

        // Act
        var dto = new ApiResponseFlatWithCollectionsDto(source);

        // Assert - Collections should be null when source is null
        dto.Items.ShouldBeNull();
        dto.Tags.ShouldBeNull();
    }
}
