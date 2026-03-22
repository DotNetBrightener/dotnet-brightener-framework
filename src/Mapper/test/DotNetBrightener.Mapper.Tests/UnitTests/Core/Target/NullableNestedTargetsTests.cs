using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

// Test entities with nullable nested properties
public class PersonEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AddressEntity? MailingAddress { get; set; }
}

public class DataTableEntity
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string StringResource { get; set; } = string.Empty;
    public DataTableExtendedDataEntity? ExtendedData { get; set; }
}

public class DataTableExtendedDataEntity
{
    public int Id { get; set; }
    public string Metadata { get; set; } = string.Empty;
}

public class OrganizationEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<PersonEntity>? OptionalMembers { get; set; }
}

// Target DTOs with nullable nested targets
[MappingTarget<DataTableExtendedDataEntity>( GenerateToSource = true)]
public partial record DataTableExtendedDataDto;

[MappingTarget<DataTableEntity>(
    NestedTargetTypes = [typeof(DataTableExtendedDataDto)],
    GenerateToSource = true)]
public partial record DataTableTargetDto;

[MappingTarget<PersonEntity>(
    NestedTargetTypes = [typeof(AddressTarget)],
    GenerateToSource = true)]
public partial record PersonDto;

[MappingTarget<OrganizationEntity>(
    NestedTargetTypes = [typeof(PersonDto)],
    GenerateToSource = true)]
public partial record OrganizationDto;

public class NullableNestedTargetsTests
{
    [Fact]
    public void Constructor_ShouldHandleNullNestedTarget_WithoutThrowingException()
    {
        // Arrange
        var dataTable = new DataTableEntity
        {
            Id = 1,
            Code = "TEST001",
            StringResource = "Test Resource",
            ExtendedData = null
        };

        // Act
        var dto = new DataTableTargetDto(dataTable);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(1);
        dto.Code.ShouldBe("TEST001");
        dto.StringResource.ShouldBe("Test Resource");
        dto.ExtendedData.ShouldBeNull();
    }

    [Fact]
    public void Constructor_ShouldMapNonNullNestedTarget_Correctly()
    {
        // Arrange
        var dataTable = new DataTableEntity
        {
            Id = 2,
            Code = "TEST002",
            StringResource = "Another Resource",
            ExtendedData = new DataTableExtendedDataEntity
            {
                Id = 100,
                Metadata = "Extended metadata"
            }
        };

        // Act
        var dto = new DataTableTargetDto(dataTable);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(2);
        dto.Code.ShouldBe("TEST002");
        dto.StringResource.ShouldBe("Another Resource");
        dto.ExtendedData.ShouldNotBeNull();
        dto.ExtendedData!.Id.ShouldBe(100);
        dto.ExtendedData.Metadata.ShouldBe("Extended metadata");
    }

    [Fact]
    public void Constructor_ShouldHandleNullNestedTarget_InMultipleProperties()
    {
        // Arrange
        var person = new PersonEntity
        {
            Id = 1,
            Name = "John Doe",
            MailingAddress = null
        };

        // Act
        var dto = new PersonDto(person);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(1);
        dto.Name.ShouldBe("John Doe");
        dto.MailingAddress.ShouldBeNull();
    }

    [Fact]
    public void Constructor_ShouldMapNonNullNestedTarget_WhenProvided()
    {
        // Arrange
        var person = new PersonEntity
        {
            Id = 2,
            Name = "Jane Smith",
            MailingAddress = new AddressEntity
            {
                Street = "123 Main St",
                City = "Anytown",
                State = "CA",
                ZipCode = "12345",
                Country = "USA"
            }
        };

        // Act
        var dto = new PersonDto(person);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(2);
        dto.Name.ShouldBe("Jane Smith");
        dto.MailingAddress.ShouldNotBeNull();
        dto.MailingAddress!.Street.ShouldBe("123 Main St");
        dto.MailingAddress.City.ShouldBe("Anytown");
    }

    [Fact]
    public void ToSource_ShouldHandleNullNestedTarget_Correctly()
    {
        // Arrange
        var dto = new DataTableTargetDto
        {
            Id = 3,
            Code = "TEST003",
            StringResource = "Resource",
            ExtendedData = null
        };

        // Act
        var entity = dto.ToSource();

        // Assert
        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(3);
        entity.Code.ShouldBe("TEST003");
        entity.StringResource.ShouldBe("Resource");
        entity.ExtendedData.ShouldBeNull();
    }

    [Fact]
    public void ToSource_ShouldMapNonNullNestedTarget_Correctly()
    {
        // Arrange
        var dto = new DataTableTargetDto
        {
            Id = 4,
            Code = "TEST004",
            StringResource = "Another",
            ExtendedData = new DataTableExtendedDataDto
            {
                Id = 200,
                Metadata = "Metadata value"
            }
        };

        // Act
        var entity = dto.ToSource();

        // Assert
        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(4);
        entity.Code.ShouldBe("TEST004");
        entity.StringResource.ShouldBe("Another");
        entity.ExtendedData.ShouldNotBeNull();
        entity.ExtendedData!.Id.ShouldBe(200);
        entity.ExtendedData.Metadata.ShouldBe("Metadata value");
    }

    [Fact]
    public void Constructor_ShouldHandleNullCollectionNestedTarget_WithoutThrowingException()
    {
        // Arrange
        var org = new OrganizationEntity
        {
            Id = 1,
            Name = "Test Org",
            OptionalMembers = null
        };

        // Act
        var dto = new OrganizationDto(org);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(1);
        dto.Name.ShouldBe("Test Org");
        dto.OptionalMembers.ShouldBeNull();
    }

    [Fact]
    public void Constructor_ShouldMapNonNullCollectionNestedTarget_Correctly()
    {
        // Arrange
        var org = new OrganizationEntity
        {
            Id = 2,
            Name = "Another Org",
            OptionalMembers =
            [
                new PersonEntity
                {
                    Id             = 1,
                    Name           = "Person 1",
                    MailingAddress = null
                },

                new PersonEntity
                {
                    Id   = 2,
                    Name = "Person 2",
                    MailingAddress = new AddressEntity
                    {
                        City = "Test City"
                    }
                }
            ]
        };

        // Act
        var dto = new OrganizationDto(org);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(2);
        dto.Name.ShouldBe("Another Org");
        dto.OptionalMembers.ShouldNotBeNull();
        dto.OptionalMembers.Count().ShouldBe(2);
        dto.OptionalMembers![0].Id.ShouldBe(1);
        dto.OptionalMembers[0].Name.ShouldBe("Person 1");
        dto.OptionalMembers[0].MailingAddress.ShouldBeNull();
        dto.OptionalMembers[1].Id.ShouldBe(2);
        dto.OptionalMembers[1].Name.ShouldBe("Person 2");
        dto.OptionalMembers[1].MailingAddress.ShouldNotBeNull();
        dto.OptionalMembers[1].MailingAddress!.City.ShouldBe("Test City");
    }

    [Fact]
    public void ToSource_ShouldHandleNullCollectionNestedTarget_Correctly()
    {
        // Arrange
        var dto = new OrganizationDto
        {
            Id = 3,
            Name = "Org 3",
            OptionalMembers = null
        };

        // Act
        var entity = dto.ToSource();

        // Assert
        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(3);
        entity.Name.ShouldBe("Org 3");
        entity.OptionalMembers.ShouldBeNull();
    }

    [Fact]
    public void Projection_ShouldHandleNullNestedTarget_InLinqQuery()
    {
        // Arrange
        var dataTables = new[]
        {
            new DataTableEntity
            {
                Id = 1,
                Code = "A001",
                StringResource = "Resource A",
                ExtendedData = null
            },
            new DataTableEntity
            {
                Id = 2,
                Code = "B001",
                StringResource = "Resource B",
                ExtendedData = new DataTableExtendedDataEntity { Id = 100, Metadata = "Meta B" }
            }
        }.AsQueryable();

        // Act
        var dtos = dataTables.Select(DataTableTargetDto.Projection).ToList();

        // Assert
        dtos.Count().ShouldBe(2);
        dtos[0].Id.ShouldBe(1);
        dtos[0].ExtendedData.ShouldBeNull();
        dtos[1].Id.ShouldBe(2);
        dtos[1].ExtendedData.ShouldNotBeNull();
        dtos[1].ExtendedData!.Metadata.ShouldBe("Meta B");
    }
}
