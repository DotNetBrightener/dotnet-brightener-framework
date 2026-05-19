using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

public class EnumConversionTests
{
    [Fact]
    public void ConvertEnumsTo_String_ShouldGenerateStringProperty()
    {
        // Arrange
        var entity = new UserWithEnum
        {
            Id = 1,
            Name = "John",
            Status = UserStatus.Active,
            Email = "john@example.com"
        };

        // Act
        var dto = new UserWithEnumToStringDto(entity);

        // Assert
        dto.Id.ShouldBe(1);
        dto.Name.ShouldBe("John");
        dto.Status.ShouldBe("Active");
        dto.Email.ShouldBe("john@example.com");
    }

    [Theory]
    [InlineData(UserStatus.Active, "Active")]
    [InlineData(UserStatus.Inactive, "Inactive")]
    [InlineData(UserStatus.Pending, "Pending")]
    [InlineData(UserStatus.Suspended, "Suspended")]
    public void ConvertEnumsTo_String_ShouldMapAllEnumValues(UserStatus status, string expected)
    {
        // Arrange
        var entity = new UserWithEnum { Status = status };

        // Act
        var dto = new UserWithEnumToStringDto(entity);

        // Assert
        dto.Status.ShouldBe(expected);
    }

    [Fact]
    public void ConvertEnumsTo_String_PropertyType_ShouldBeString()
    {
        // Assert that the generated property type is string, not the enum
        var statusProperty = typeof(UserWithEnumToStringDto).GetProperty("Status");
        statusProperty.ShouldNotBeNull();
        statusProperty!.PropertyType.ShouldBe(typeof(string));
    }

    [Fact]
    public void ConvertEnumsTo_String_NonEnumProperties_ShouldBeUnchanged()
    {
        // Verify non-enum properties are not affected
        var idProperty = typeof(UserWithEnumToStringDto).GetProperty("Id");
        idProperty!.PropertyType.ShouldBe(typeof(int));

        var nameProperty = typeof(UserWithEnumToStringDto).GetProperty("Name");
        nameProperty!.PropertyType.ShouldBe(typeof(string));
    }

    [Fact]
    public void ConvertEnumsTo_String_FromSource_ShouldWork()
    {
        // Arrange
        var entity = new UserWithEnum { Status = UserStatus.Pending };

        // Act
        var dto = UserWithEnumToStringDto.FromSource(entity);

        // Assert
        dto.Status.ShouldBe("Pending");
    }

    [Fact]
    public void ConvertEnumsTo_String_ToSource_ShouldConvertBack()
    {
        // Arrange
        var dto = new UserWithEnumToStringDto
        {
            Id = 1,
            Name = "John",
            Status = "Active",
            Email = "john@example.com"
        };

        // Act
        var entity = dto.ToSource();

        // Assert
        entity.Status.ShouldBe(UserStatus.Active);
        entity.Id.ShouldBe(1);
        entity.Name.ShouldBe("John");
    }

    [Fact]
    public void ConvertEnumsTo_Int_ShouldGenerateIntProperty()
    {
        // Arrange
        var entity = new UserWithEnum
        {
            Id = 1,
            Name = "John",
            Status = UserStatus.Active,
            Email = "john@example.com"
        };

        // Act
        var dto = new UserWithEnumToIntDto(entity);

        // Assert
        dto.Id.ShouldBe(1);
        dto.Name.ShouldBe("John");
        dto.Status.ShouldBe(0); // Active = 0
        dto.Email.ShouldBe("john@example.com");
    }

    [Theory]
    [InlineData(UserStatus.Active, 0)]
    [InlineData(UserStatus.Inactive, 1)]
    [InlineData(UserStatus.Pending, 2)]
    [InlineData(UserStatus.Suspended, 3)]
    public void ConvertEnumsTo_Int_ShouldMapAllEnumValues(UserStatus status, int expected)
    {
        // Arrange
        var entity = new UserWithEnum { Status = status };

        // Act
        var dto = new UserWithEnumToIntDto(entity);

        // Assert
        dto.Status.ShouldBe(expected);
    }

    [Fact]
    public void ConvertEnumsTo_Int_PropertyType_ShouldBeInt()
    {
        var statusProperty = typeof(UserWithEnumToIntDto).GetProperty("Status");
        statusProperty.ShouldNotBeNull();
        statusProperty!.PropertyType.ShouldBe(typeof(int));
    }

    [Fact]
    public void ConvertEnumsTo_Int_ToSource_ShouldConvertBack()
    {
        // Arrange
        var dto = new UserWithEnumToIntDto
        {
            Id = 1,
            Name = "John",
            Status = 2, // Pending
            Email = "john@example.com"
        };

        // Act
        var entity = dto.ToSource();

        // Assert
        entity.Status.ShouldBe(UserStatus.Pending);
    }

    [Fact]
    public void ConvertEnumsTo_String_NullableEnum_ShouldHandleNonNullValue()
    {
        // Arrange
        var entity = new EntityWithNullableEnum
        {
            Id = 1,
            Name = "Test",
            Status = UserStatus.Active,
            NonNullableStatus = UserStatus.Pending
        };

        // Act
        var dto = new NullableEnumToStringDto(entity);

        // Assert
        dto.Status.ShouldBe("Active");
        dto.NonNullableStatus.ShouldBe("Pending");
    }

    [Fact]
    public void ConvertEnumsTo_String_NullableEnum_ShouldHandleNullValue()
    {
        // Arrange
        var entity = new EntityWithNullableEnum
        {
            Id = 1,
            Name = "Test",
            Status = null,
            NonNullableStatus = UserStatus.Active
        };

        // Act
        var dto = new NullableEnumToStringDto(entity);

        // Assert
        dto.Status.ShouldBeNull();
        dto.NonNullableStatus.ShouldBe("Active");
    }

    [Fact]
    public void ConvertEnumsTo_String_NullableEnum_PropertyType_ShouldBeNullableString()
    {
        var statusProperty = typeof(NullableEnumToStringDto).GetProperty("Status");
        statusProperty.ShouldNotBeNull();
        statusProperty!.PropertyType.ShouldBe(typeof(string));
    }

    [Fact]
    public void ConvertEnumsTo_Int_NullableEnum_ShouldHandleNonNullValue()
    {
        // Arrange
        var entity = new EntityWithNullableEnum
        {
            Id = 1,
            Name = "Test",
            Status = UserStatus.Suspended,
            NonNullableStatus = UserStatus.Active
        };

        // Act
        var dto = new NullableEnumToIntDto(entity);

        // Assert
        dto.Status.ShouldBe(3); // Suspended = 3
        dto.NonNullableStatus.ShouldBe(0); // Active = 0
    }

    [Fact]
    public void ConvertEnumsTo_Int_NullableEnum_ShouldHandleNullValue()
    {
        // Arrange
        var entity = new EntityWithNullableEnum
        {
            Id = 1,
            Name = "Test",
            Status = null,
            NonNullableStatus = UserStatus.Active
        };

        // Act
        var dto = new NullableEnumToIntDto(entity);

        // Assert
        dto.Status.ShouldBeNull();
        dto.NonNullableStatus.ShouldBe(0);
    }

    [Fact]
    public void ConvertEnumsTo_Int_NullableEnum_PropertyType_ShouldBeNullableInt()
    {
        var statusProperty = typeof(NullableEnumToIntDto).GetProperty("Status");
        statusProperty.ShouldNotBeNull();
        statusProperty!.PropertyType.ShouldBe(typeof(int?));
    }

    [Fact]
    public void ConvertEnumsTo_String_WithNullableProperties_ShouldMakeAllNullable()
    {
        // Arrange
        var entity = new UserWithEnum
        {
            Id = 1,
            Name = "John",
            Status = UserStatus.Active,
            Email = "john@example.com"
        };

        // Act
        var dto = new UserWithEnumToStringNullableDto(entity);

        // Assert
        dto.Status.ShouldBe("Active");

        // Verify the property is nullable string
        var statusProperty = typeof(UserWithEnumToStringNullableDto).GetProperty("Status");
        statusProperty.ShouldNotBeNull();
        statusProperty!.PropertyType.ShouldBe(typeof(string)); // string is already nullable ref type
    }

    [Fact]
    public void ConvertEnumsTo_String_Projection_ShouldExist()
    {
        // Verify the Projection property exists and is not null
        var projection = UserWithEnumToStringDto.Projection;
        projection.ShouldNotBeNull();
    }

    [Fact]
    public void ConvertEnumsTo_String_Projection_ShouldMapCorrectly()
    {
        // Arrange
        var entities = new List<UserWithEnum>
        {
            new() { Id = 1, Name = "Alice", Status = UserStatus.Active, Email = "alice@test.com" },
            new() { Id = 2, Name = "Bob", Status = UserStatus.Pending, Email = "bob@test.com" }
        };

        // Act
        var dtos = entities.AsQueryable()
            .Select(UserWithEnumToStringDto.Projection)
            .ToList();

        // Assert
        dtos.Count().ShouldBe(2);
        dtos[0].Status.ShouldBe("Active");
        dtos[1].Status.ShouldBe("Pending");
    }

    [Fact]
    public void ConvertEnumsTo_Int_Projection_ShouldMapCorrectly()
    {
        // Arrange
        var entities = new List<UserWithEnum>
        {
            new() { Id = 1, Name = "Alice", Status = UserStatus.Active, Email = "alice@test.com" },
            new() { Id = 2, Name = "Bob", Status = UserStatus.Suspended, Email = "bob@test.com" }
        };

        // Act
        var dtos = entities.AsQueryable()
            .Select(UserWithEnumToIntDto.Projection)
            .ToList();

        // Assert
        dtos.Count().ShouldBe(2);
        dtos[0].Status.ShouldBe(0); // Active
        dtos[1].Status.ShouldBe(3); // Suspended
    }

    [Theory]
    [InlineData(UserStatus.Active)]
    [InlineData(UserStatus.Inactive)]
    [InlineData(UserStatus.Pending)]
    [InlineData(UserStatus.Suspended)]
    public void ConvertEnumsTo_String_RoundTrip_ShouldPreserveValue(UserStatus status)
    {
        // Arrange
        var original = new UserWithEnum
        {
            Id = 42,
            Name = "Test",
            Status = status,
            Email = "test@test.com"
        };

        // Act - forward mapping
        var dto = new UserWithEnumToStringDto(original);

        // Act - reverse mapping
        var result = dto.ToSource();

        // Assert
        result.Status.ShouldBe(status);
        result.Id.ShouldBe(42);
        result.Name.ShouldBe("Test");
    }

    [Theory]
    [InlineData(UserStatus.Active)]
    [InlineData(UserStatus.Inactive)]
    [InlineData(UserStatus.Pending)]
    [InlineData(UserStatus.Suspended)]
    public void ConvertEnumsTo_Int_RoundTrip_ShouldPreserveValue(UserStatus status)
    {
        // Arrange
        var original = new UserWithEnum
        {
            Id = 42,
            Name = "Test",
            Status = status,
            Email = "test@test.com"
        };

        // Act - forward mapping
        var dto = new UserWithEnumToIntDto(original);

        // Act - reverse mapping
        var result = dto.ToSource();

        // Assert
        result.Status.ShouldBe(status);
    }
}
