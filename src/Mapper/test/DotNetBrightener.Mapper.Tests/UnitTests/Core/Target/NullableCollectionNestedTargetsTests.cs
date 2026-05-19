namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

public class StringLookup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class StringIdentifier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<StringLookup> StringLookups { get; set; } = [];
}

// Target DTOs
[MappingTarget<StringLookup>( NullableProperties = true, GenerateToSource = true)]
public partial class StringLookupDto;

[MappingTarget<StringIdentifier>(
    Include = [nameof(StringIdentifier.Id), nameof(StringIdentifier.Name), nameof(StringIdentifier.StringLookups)],
    NestedTargetTypes = [typeof(StringLookupDto)],
    NullableProperties = true,
    GenerateToSource = true)]
public partial class StringIdentifierLookupDto;

public class NullableCollectionNestedTargetsTests
{
    [Fact]
    public void Constructor_ShouldHandleCollectionNestedTarget_WithNullableProperties()
    {
        // Arrange
        var stringIdentifier = new StringIdentifier
        {
            Id = 1,
            Name = "Test Identifier",
            StringLookups =
            [
                new()
                {
                    Id    = 10,
                    Name  = "Lookup1",
                    Value = "Value1"
                },
                new()
                {
                    Id    = 20,
                    Name  = "Lookup2",
                    Value = "Value2"
                }
            ]
        };

        // Act
        var dto = new StringIdentifierLookupDto(stringIdentifier);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(1);
        dto.Name.ShouldBe("Test Identifier");
        dto.StringLookups.ShouldNotBeNull();
        dto.StringLookups.Count().ShouldBe(2);
    }

    [Fact]
    public void Projection_ShouldHandleCollectionNestedTarget_WithNullableProperties()
    {
        // Arrange
        var identifiers = new[]
        {
            new StringIdentifier
            {
                Id            = 1,
                Name          = "Identifier 1",
                StringLookups =
                [
                    new()
                    {
                        Id    = 10,
                        Name  = "Lookup1",
                        Value = "Value1"
                    }
                ]
            }
        }.AsQueryable();

        // Act
        var dtos = identifiers.Select(StringIdentifierLookupDto.Projection).ToList();

        // Assert
        dtos.Count().ShouldBe(1);
        dtos[0].Id.ShouldBe(1);
        dtos[0].StringLookups.ShouldNotBeNull();
        dtos[0].StringLookups!.Count().ShouldBe(1);
    }

    [Fact]
    public void ToSource_ShouldHandleCollectionNestedTarget_WithNullableProperties()
    {
        // Arrange
        var dto = new StringIdentifierLookupDto
        {
            Id = 1,
            Name = "Test Identifier",
            StringLookups =
            [
                new()
                {
                    Id    = 10,
                    Name  = "Lookup1",
                    Value = "Value1"
                },
                new()
                {
                    Id    = 20,
                    Name  = "Lookup2",
                    Value = "Value2"
                }
            ]
        };

        // Act
        var entity = dto.ToSource();

        // Assert
        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(1);
        entity.Name.ShouldBe("Test Identifier");
        entity.StringLookups.Count().ShouldBe(2);
    }
}
