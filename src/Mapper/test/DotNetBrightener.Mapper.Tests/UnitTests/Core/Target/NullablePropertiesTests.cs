using DotNetBrightener.Mapper.Tests.TestModels;
using DotNetBrightener.Mapper.Tests.Utilities;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

public class NullablePropertiesTests
{
    [Fact]
    public void ProductQueryDto_ShouldHaveAllPropertiesNullable_WhenNullablePropertiesIsTrue()
    {
        // Arrange & Act
        var dtoType = typeof(ProductQueryDto);

        // Assert
        var idProp = dtoType.GetProperty("Id");
        idProp.ShouldNotBeNull();
        idProp!.PropertyType.ShouldBe(typeof(int?), "Id should be nullable int");

        var nameProp = dtoType.GetProperty("Name");
        nameProp.ShouldNotBeNull();
        nameProp!.PropertyType.ShouldBe(typeof(string), "Name is a reference type");

        var descriptionProp = dtoType.GetProperty("Description");
        descriptionProp.ShouldNotBeNull();
        descriptionProp!.PropertyType.ShouldBe(typeof(string), "Description is a reference type");

        var priceProp = dtoType.GetProperty("Price");
        priceProp.ShouldNotBeNull();
        priceProp!.PropertyType.ShouldBe(typeof(decimal?), "Price should be nullable decimal");

        var categoryIdProp = dtoType.GetProperty("CategoryId");
        categoryIdProp.ShouldNotBeNull();
        categoryIdProp!.PropertyType.ShouldBe(typeof(int?), "CategoryId should be nullable int");

        var isAvailableProp = dtoType.GetProperty("IsAvailable");
        isAvailableProp.ShouldNotBeNull();
        isAvailableProp!.PropertyType.ShouldBe(typeof(bool?), "IsAvailable should be nullable bool");

        // Excluded properties should not exist
        dtoType.GetProperty("InternalNotes").ShouldBeNull("InternalNotes should be excluded");
        dtoType.GetProperty("CreatedAt").ShouldBeNull("CreatedAt should be excluded");
    }

    [Fact]
    public void UserQueryDto_ShouldHaveAllPropertiesNullable_WhenNullablePropertiesIsTrue()
    {
        // Arrange & Act
        var dtoType = typeof(UserQueryDto);

        // Assert - All properties should be nullable
        var idProp = dtoType.GetProperty("Id");
        idProp.ShouldNotBeNull();
        idProp!.PropertyType.ShouldBe(typeof(int?), "Id should be nullable int");

        var firstNameProp = dtoType.GetProperty("FirstName");
        firstNameProp.ShouldNotBeNull();
        firstNameProp!.PropertyType.ShouldBe(typeof(string), "FirstName is a reference type");

        var lastNameProp = dtoType.GetProperty("LastName");
        lastNameProp.ShouldNotBeNull();
        lastNameProp!.PropertyType.ShouldBe(typeof(string), "LastName is a reference type");

        var emailProp = dtoType.GetProperty("Email");
        emailProp.ShouldNotBeNull();
        emailProp!.PropertyType.ShouldBe(typeof(string), "Email is a reference type");

        var dateOfBirthProp = dtoType.GetProperty("DateOfBirth");
        dateOfBirthProp.ShouldNotBeNull();
        dateOfBirthProp!.PropertyType.ShouldBe(typeof(DateTime?), "DateOfBirth should be nullable DateTime");

        var isActiveProp = dtoType.GetProperty("IsActive");
        isActiveProp.ShouldNotBeNull();
        isActiveProp!.PropertyType.ShouldBe(typeof(bool?), "IsActive should be nullable bool");

        var lastLoginAtProp = dtoType.GetProperty("LastLoginAt");
        lastLoginAtProp.ShouldNotBeNull();
        lastLoginAtProp!.PropertyType.ShouldBe(typeof(DateTime?), "LastLoginAt should remain nullable DateTime");

        // Excluded properties should not exist
        dtoType.GetProperty("Password").ShouldBeNull("Password should be excluded");
        dtoType.GetProperty("CreatedAt").ShouldBeNull("CreatedAt should be excluded");
    }

    [Fact]
    public void UserWithEnumQueryDto_ShouldHaveEnumAsNullable_WhenNullablePropertiesIsTrue()
    {
        // Arrange & Act
        var dtoType = typeof(UserWithEnumQueryDto);

        // Assert - Enum property should be nullable
        var statusProp = dtoType.GetProperty("Status");
        statusProp.ShouldNotBeNull();
        statusProp!.PropertyType.ShouldBe(typeof(UserStatus?), "Status should be nullable UserStatus enum");

        var idProp = dtoType.GetProperty("Id");
        idProp.ShouldNotBeNull();
        idProp!.PropertyType.ShouldBe(typeof(int?), "Id should be nullable int");

        var nameProp = dtoType.GetProperty("Name");
        nameProp.ShouldNotBeNull();
        nameProp!.PropertyType.ShouldBe(typeof(string), "Name is a reference type");

        var emailProp = dtoType.GetProperty("Email");
        emailProp.ShouldNotBeNull();
        emailProp!.PropertyType.ShouldBe(typeof(string), "Email is a reference type");
    }

    [Fact]
    public void ProductQueryDto_ShouldCreateInstance_WithNullValues()
    {
        // Arrange & Act
        var dto = new ProductQueryDto();

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBeNull("Id should default to null");
        dto.Name.ShouldBeNull("Name should default to null");
        dto.Description.ShouldBeNull("Description should default to null");
        dto.Price.ShouldBeNull("Price should default to null");
        dto.CategoryId.ShouldBeNull("CategoryId should default to null");
        dto.IsAvailable.ShouldBeNull("IsAvailable should default to null");
    }

    [Fact]
    public void ProductQueryDto_ShouldMapFromSource_WithNullableProperties()
    {
        // Arrange
        var product = TestDataFactory.CreateProduct("Test Product", 99.99m);

        // Act
        var dto = new ProductQueryDto(product);

        // Assert
        dto.Id.ShouldBe(product.Id);
        dto.Name.ShouldBe(product.Name);
        dto.Description.ShouldBe(product.Description);
        dto.Price.ShouldBe(product.Price);
        dto.CategoryId.ShouldBe(product.CategoryId);
        dto.IsAvailable.ShouldBe(product.IsAvailable);
    }

    [Fact]
    public void UserQueryDto_ShouldMapFromSource_WithNullableProperties()
    {
        // Arrange
        var user = TestDataFactory.CreateUser("Jane", "Doe", "jane@example.com");

        // Act
        var dto = new UserQueryDto(user);

        // Assert - Values should be mapped correctly
        dto.Id.ShouldBe(user.Id);
        dto.FirstName.ShouldBe(user.FirstName);
        dto.LastName.ShouldBe(user.LastName);
        dto.Email.ShouldBe(user.Email);
        dto.DateOfBirth.ShouldBe(user.DateOfBirth);
        dto.IsActive.ShouldBe(user.IsActive);
        dto.LastLoginAt.ShouldBe(user.LastLoginAt);
    }

    [Fact]
    public void ProductQueryDto_ShouldAllowPartialData_ForQueryScenarios()
    {
        // Arrange & Act
        var queryDto = new ProductQueryDto
        {
            Name = "Test",
            Price = 50.00m
            // Other fields remain null
        };

        // Assert
        queryDto.Name.ShouldBe("Test");
        queryDto.Price.ShouldBe(50.00m);
        queryDto.Id.ShouldBeNull();
        queryDto.Description.ShouldBeNull();
        queryDto.CategoryId.ShouldBeNull();
        queryDto.IsAvailable.ShouldBeNull();
    }
}
