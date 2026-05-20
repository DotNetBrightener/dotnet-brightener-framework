using System.Reflection;
using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Tests.TestModels;
using DotNetBrightener.Mapper.Tests.Utilities;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

public class IncludePropertyTests
{
    [Fact]
    public void ToTarget_WithInclude_ShouldOnlyIncludeSpecifiedProperties()
    {
        // Arrange
        var user = TestDataFactory.CreateUser("John", "Doe");

        // Act
        var dto = user.ToTarget<User, UserIncludeDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.FirstName.ShouldBe("John");
        dto.LastName.ShouldBe("Doe");
        dto.Email.ShouldBe(user.Email);

        // Verify that excluded properties are not present
        var dtoType = dto.GetType();
        dtoType.GetProperty("Id").ShouldBeNull("Id should not be included");
        dtoType.GetProperty("DateOfBirth").ShouldBeNull("DateOfBirth should not be included");
        dtoType.GetProperty("Password").ShouldBeNull("Password should not be included");
        dtoType.GetProperty("IsActive").ShouldBeNull("IsActive should not be included");
        dtoType.GetProperty("CreatedAt").ShouldBeNull("CreatedAt should not be included");
        dtoType.GetProperty("LastLoginAt").ShouldBeNull("LastLoginAt should not be included");
    }

    [Fact]
    public void ToTarget_WithInclude_ShouldWorkWithSingleProperty()
    {
        // Arrange
        var user = TestDataFactory.CreateUser("Jane", "Smith");

        // Act
        var dto = user.ToTarget<User, UserSingleIncludeDto>();


        // Assert
        dto.ShouldNotBeNull();
        dto.FirstName.ShouldBe("Jane");
        // Verify that all other properties are not present
        var dtoType = dto.GetType();
        dtoType.GetProperty("LastName").ShouldBeNull("LastName should not be included");
        dtoType.GetProperty("Email").ShouldBeNull("Email should not be included");
        dtoType.GetProperty("Id").ShouldBeNull("Id should not be included");
    }

    [Fact]
    public void ToTarget_WithInclude_ShouldWorkWithSingleObjectProperty()
    {
        // Arrange
        var user = TestDataFactory.CreateUser("Jane", "Smith", "", DateTime.Today);

        // Act
        var dto = user.ToTarget<User, UserSingleObjectIncludeDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.DateOfBirth.ShouldBe(DateTime.Today);

        // Verify that all other properties are not present
        var dtoType = dto.GetType();
        dtoType.GetProperty("LastName").ShouldBeNull("LastName should not be included");
        dtoType.GetProperty("Email").ShouldBeNull("Email should not be included");
        dtoType.GetProperty("Id").ShouldBeNull("Id should not be included");
    }

    [Fact]
    public void ToTarget_ShouldWorkWithSingleObjectProperty()
    {
        var id = Guid.NewGuid();
        // Arrange
        var tenant = TestDataFactory.CreateTenant(id);

        // Act
        var dto = tenant.ToTarget<Tenant, TenantSingleObjectIncludeDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(id);

        // Verify that all properties are present
        var dtoType = dto.GetType();
        dtoType.GetProperty("Id").ShouldNotBeNull("LastName should be included");
    }

    [Fact]
    public void ToTarget_WithInclude_ShouldWorkWithProductEntity()
    {
        // Arrange
        var product = TestDataFactory.CreateProduct("Test Product", 99.99m);

        // Act
        var dto = product.ToTarget<Product, ProductIncludeDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.Name.ShouldBe("Test Product");
        dto.Price.ShouldBe(99.99m);

        // Verify that excluded properties are not present
        var dtoType = dto.GetType();
        dtoType.GetProperty("Id").ShouldBeNull("Id should not be included");
        dtoType.GetProperty("Description").ShouldBeNull("Description should not be included");
        dtoType.GetProperty("CategoryId").ShouldBeNull("CategoryId should not be included");
        dtoType.GetProperty("IsAvailable").ShouldBeNull("IsAvailable should not be included");
        dtoType.GetProperty("CreatedAt").ShouldBeNull("CreatedAt should not be included");
        dtoType.GetProperty("InternalNotes").ShouldBeNull("InternalNotes should not be included");
    }

    [Fact]
    public void ToTarget_WithInclude_ShouldPreservePropertyTypes()
    {
        // Arrange
        var user = TestDataFactory.CreateUser("Type", "Test");

        // Act
        var dto = user.ToTarget<User, UserIncludeDto>();

        // Assert
        var dtoType = dto.GetType();
        var firstNameProp = dtoType.GetProperty("FirstName");
        var lastNameProp = dtoType.GetProperty("LastName");
        var emailProp = dtoType.GetProperty("Email");

        firstNameProp.ShouldNotBeNull();
        firstNameProp!.PropertyType.ShouldBe(typeof(string));

        lastNameProp.ShouldNotBeNull();
        lastNameProp!.PropertyType.ShouldBe(typeof(string));

        emailProp.ShouldNotBeNull();
        emailProp!.PropertyType.ShouldBe(typeof(string));
    }

    [Fact]
    public void ToTarget_WithInclude_ShouldWorkWithInheritedProperties()
    {
        // Arrange
        var employee = TestDataFactory.CreateEmployee("Include", "Test", "Engineering");

        // Act
        var dto = employee.ToTarget<Employee, EmployeeIncludeDto>();

        // Assert
        ShouldBeNullExtensions.ShouldNotBeNull<EmployeeIncludeDto>(dto);
        dto.FirstName.ShouldBe("Include"); // From User base class
        dto.LastName.ShouldBe("Test"); // From User base class
        dto.Department.ShouldBe("Engineering"); // From Employee class

        // Verify excluded properties are not present
        var dtoType = dto.GetType();
        ShouldBeNullExtensions.ShouldBeNull<PropertyInfo>(dtoType.GetProperty("Id"), "Id should not be included");
        ShouldBeNullExtensions.ShouldBeNull<PropertyInfo>(dtoType.GetProperty("Email"), "Email should not be included");
        ShouldBeNullExtensions.ShouldBeNull<PropertyInfo>(dtoType.GetProperty("EmployeeId"), "EmployeeId should not be included");
        ShouldBeNullExtensions.ShouldBeNull<PropertyInfo>(dtoType.GetProperty("Salary"), "Salary should not be included");
    }

    [Fact]
    public void ToTarget_WithInclude_AndCustomProperties_ShouldWork()
    {
        // Arrange
        var user = TestDataFactory.CreateUser("Custom", "Props");

        // Act  
        var dto = user.ToTarget<User, UserIncludeWithCustomDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.FirstName.ShouldBe("Custom");
        dto.LastName.ShouldBe("Props");

        // Custom property should exist and have default value
        dto.FullName.ShouldBe(string.Empty);

        // Verify excluded properties are not present
        var dtoType = dto.GetType();
        dtoType.GetProperty("Email").ShouldBeNull("Email should not be included");
        dtoType.GetProperty("Id").ShouldBeNull("Id should not be included");
    }

    [Fact]
    public void ToTarget_WithInclude_ShouldSupportModernRecordTypes()
    {
        // Arrange
        var modernUser = TestDataFactory.CreateModernUser("Modern", "Include");

        // Act
        var dto = modernUser.ToTarget<ModernUser, ModernUserIncludeDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.FirstName.ShouldBe("Modern");
        dto.LastName.ShouldBe("Include");

        // Verify excluded properties are not present
        var dtoType = dto.GetType();
        dtoType.GetProperty("Id").ShouldBeNull("Id should not be included");
        dtoType.GetProperty("Email").ShouldBeNull("Email should not be included");
        dtoType.GetProperty("CreatedAt").ShouldBeNull("CreatedAt should not be included");
        dtoType.GetProperty("Bio").ShouldBeNull("Bio should not be included");
        dtoType.GetProperty("PasswordHash").ShouldBeNull("PasswordHash should not be included");
    }

    [Fact]
    public void ToTarget_WithInclude_ShouldWorkWithFields_WhenIncludeFieldsIsTrue()
    {
        // Arrange  
        var fieldEntity = new EntityWithFields
        {
            Name = "Field Test",
            Age = 25,
            Email = "test@test.com",
            Id = 1
        };

        // Act
        var dto = fieldEntity.ToTarget<EntityWithFields, EntityWithFieldsIncludeDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.Name.ShouldBe("Field Test");
        dto.Age.ShouldBe(25);

        // Verify excluded field is not present
        var dtoType = dto.GetType();
        dtoType.GetField("Id").ShouldBeNull("Id field should not be included");
        dtoType.GetProperty("Email").ShouldBeNull("Email property should not be included");
    }

    [Fact]
    public void ToTarget_WithInclude_ShouldNotIncludeFields_WhenIncludeFieldsIsFalse()
    {
        // This test verifies that IncludeFields defaults to false for include mode
        // Arrange  
        var fieldEntity = new EntityWithFields
        {
            Name = "Field Test",
            Age = 25,
            Email = "test@test.com",
            Id = 1
        };

        // Act
        var dto = fieldEntity.ToTarget<EntityWithFields, EntityWithFieldsIncludeNoFieldsDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.Email.ShouldBe("test@test.com"); // Property should be included

        // Verify fields are not included even if specified in include array
        var dtoType = dto.GetType();
        dtoType.GetField("Name").ShouldBeNull("Name field should not be included when IncludeFields = false");
        dtoType.GetField("Age").ShouldBeNull("Age field should not be included when IncludeFields = false");
    }

    [Fact]
    public void BackTo_WithInclude_ShouldCreateSourceWithDefaultValues()
    {
        // Arrange
        var user = TestDataFactory.CreateUser("Back", "To");
        var dto = user.ToTarget<User, UserIncludeDto>();

        // Act
        var backToSource = dto.BackTo();

        // Assert
        backToSource.ShouldNotBeNull();
        backToSource.FirstName.ShouldBe("Back");
        backToSource.LastName.ShouldBe("To");
        backToSource.Email.ShouldBe(user.Email);

        // Properties not included in target should have default values
        backToSource.Id.ShouldBe(0); // Default for int
        backToSource.DateOfBirth.ShouldBe(default(DateTime));
        backToSource.Password.ShouldBe(string.Empty); // Default for string
        backToSource.IsActive.ShouldBeFalse(); // Default for bool
        backToSource.CreatedAt.ShouldBe(default(DateTime));
        backToSource.LastLoginAt.ShouldBeNull(); // Nullable property
    }
}
