using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Tests.TestModels;
using DotNetBrightener.Mapper.Tests.Utilities;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

public class BasicMappingTests
{
    [Fact]
    public void ToTarget_ShouldMapBasicProperties_WhenMappingUserToDto()
    {
        // Arrange
        var user = TestDataFactory.CreateUser("John", "Doe", "john@example.com");

        // Act
        var dto = user.ToTarget<User, UserDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(user.Id);
        dto.FirstName.ShouldBe("John");
        dto.LastName.ShouldBe("Doe");
        dto.Email.ShouldBe("john@example.com");
        dto.IsActive.ShouldBe(user.IsActive);
        dto.DateOfBirth.ShouldBe(user.DateOfBirth);
        dto.LastLoginAt.ShouldBe(user.LastLoginAt);
    }

    [Fact]
    public void ToTarget_ShouldExcludeSpecifiedProperties_WhenMappingUser()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();

        // Act
        var dto = user.ToTarget<User, UserDto>();

        // Assert
        var dtoType = dto.GetType();
        dtoType.GetProperty("Password").ShouldBeNull("Password should be excluded");
        dtoType.GetProperty("CreatedAt").ShouldBeNull("CreatedAt should be excluded");
    }

    [Fact]
    public void ToTarget_ShouldMapProductProperties_ExcludingInternalNotes()
    {
        // Arrange
        var product = TestDataFactory.CreateProduct("Test Product", 49.99m);

        // Act
        var dto = product.ToTarget<Product, ProductDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(product.Id);
        dto.Name.ShouldBe("Test Product");
        dto.Description.ShouldBe(product.Description);
        dto.Price.ShouldBe(49.99m);
        dto.IsAvailable.ShouldBe(product.IsAvailable);
        
        var dtoType = dto.GetType();
        dtoType.GetProperty("InternalNotes").ShouldBeNull("InternalNotes should be excluded");
    }

    [Fact]
    public void ToTarget_ShouldHandleNullableProperties_Correctly()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        user.LastLoginAt = null;

        // Act
        var dto = user.ToTarget<User, UserDto>();

        // Assert
        dto.LastLoginAt.ShouldBeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ToTarget_ShouldPreserveBooleanValues_ForIsActiveProperty(bool isActive)
    {
        // Arrange
        var user = TestDataFactory.CreateUser(isActive: isActive);

        // Act
        var dto = user.ToTarget<User, UserDto>();

        // Assert
        dto.IsActive.ShouldBe(isActive);
    }

    [Fact]
    public void ToTarget_ShouldMapMultipleUsers_WithDifferentData()
    {
        // Arrange
        var users = TestDataFactory.CreateUsers();

        // Act
        var dtos = users.Select(u => u.ToTarget<User, UserDto>()).ToList();

        // Assert
        dtos.Count().ShouldBe(3);
        dtos[0].FirstName.ShouldBe("Alice");
        dtos[1].FirstName.ShouldBe("Bob");
        dtos[2].FirstName.ShouldBe("Charlie");
        dtos[2].IsActive.ShouldBeFalse();
    }
}
