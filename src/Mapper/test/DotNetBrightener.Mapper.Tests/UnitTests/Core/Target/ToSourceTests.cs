using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Tests.TestModels;
using DotNetBrightener.Mapper.Tests.Utilities;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

public class ToSourceTests
{
    #region Class Tests

    [Fact]
    public void ToSourceShorthand_ShouldMapBasicProperties_WhenMappingFromUserDto()
    {
        // Arrange
        var originalUser = TestDataFactory.CreateUser("John", "Doe", "john@example.com");
        var userDto = originalUser.ToTarget<User, UserDto>();

        // Act
        var mappedUser = userDto.ToSource<User>();

        // Assert
        mappedUser.ShouldNotBeNull();
        mappedUser.Id.ShouldBe(originalUser.Id);
        mappedUser.FirstName.ShouldBe("John");
        mappedUser.LastName.ShouldBe("Doe");
        mappedUser.Email.ShouldBe("john@example.com");
        mappedUser.IsActive.ShouldBe(originalUser.IsActive);
        mappedUser.DateOfBirth.ShouldBe(originalUser.DateOfBirth);
        mappedUser.LastLoginAt.ShouldBe(originalUser.LastLoginAt);
    }

    [Fact]
    public void ToSource_ShouldMapBasicProperties_WhenMappingFromUserDto()
    {
        // Arrange
        var originalUser = TestDataFactory.CreateUser("John", "Doe", "john@example.com");
        var userDto = originalUser.ToTarget<User, UserDto>();

        // Act
        var mappedUser = userDto.ToSource<UserDto, User>();

        // Assert
        mappedUser.ShouldNotBeNull();
        mappedUser.Id.ShouldBe(originalUser.Id);
        mappedUser.FirstName.ShouldBe("John");
        mappedUser.LastName.ShouldBe("Doe");
        mappedUser.Email.ShouldBe("john@example.com");
        mappedUser.IsActive.ShouldBe(originalUser.IsActive);
        mappedUser.DateOfBirth.ShouldBe(originalUser.DateOfBirth);
        mappedUser.LastLoginAt.ShouldBe(originalUser.LastLoginAt);
    }

    [Fact]
    public void ToSource_ShouldSetDefaultValues_ForExcludedProperties()
    {
        // Arrange
        var originalUser = TestDataFactory.CreateUser();
        var userDto = originalUser.ToTarget<User, UserDto>();

        // Act
        var mappedUser = userDto.ToSource<UserDto, User>();

        // Assert
        mappedUser.ShouldNotBeNull();
        mappedUser.Password.ShouldBeEmpty("Password was excluded from DTO");
        mappedUser.CreatedAt.ShouldBe(default(DateTime), "CreatedAt was excluded from DTO");
    }

    [Fact]
    public void ToSource_ShouldHandleNullableProperties_Correctly()
    {
        // Arrange
        var originalUser = TestDataFactory.CreateUser();
        originalUser.LastLoginAt = null;
        var userDto = originalUser.ToTarget<User, UserDto>();

        // Act
        var mappedUser = userDto.ToSource<User>();


        // Assert
        mappedUser.LastLoginAt.ShouldBeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ToSource_ShouldPreserveBooleanValues_ForIsActiveProperty(bool isActive)
    {
        // Arrange
        var originalUser = TestDataFactory.CreateUser(isActive: isActive);
        var userDto = originalUser.ToTarget<User, UserDto>();

        // Act
        var mappedUser = userDto.ToSource<User>();

        // Assert
        mappedUser.IsActive.ShouldBe(isActive);
    }

    [Fact]
    public void ToSource_ShouldHandleEmployeeDto_WithInheritedProperties()
    {
        // Arrange
        var originalEmployee = TestDataFactory.CreateEmployee("Jane", "Smith");
        var employeeDto = originalEmployee.ToTarget<Employee, EmployeeDto>();

        // Act
        var mappedEmployee = employeeDto.ToSource<Employee>();

        // Assert
        mappedEmployee.ShouldNotBeNull();
        ShouldBeStringTestExtensions.ShouldBe(mappedEmployee.FirstName, "Jane");
        ShouldBeStringTestExtensions.ShouldBe(mappedEmployee.LastName, "Smith");
        ShouldBeStringTestExtensions.ShouldBe(mappedEmployee.EmployeeId, originalEmployee.EmployeeId);
        ShouldBeStringTestExtensions.ShouldBe(mappedEmployee.Department, originalEmployee.Department);
        mappedEmployee.HireDate.ShouldBe(originalEmployee.HireDate);

        // Excluded properties should have default values
        ShouldBeEnumerableTestExtensions.ShouldBeEmpty<char>(mappedEmployee.Password);
        ShouldBeTestExtensions.ShouldBe<decimal>(mappedEmployee.Salary, 0);
        ShouldBeTestExtensions.ShouldBe(mappedEmployee.CreatedAt, default(DateTime));
    }

    [Fact]
    public void ToSource_ShouldHandleManagerDto_WithMultipleLevelsOfInheritance()
    {
        // Arrange
        var originalManager = TestDataFactory.CreateManager("Bob", "Wilson");
        var managerDto = originalManager.ToTarget<Manager, ManagerDto>();

        // Act
        var mappedManager = managerDto.ToSource<Manager>();

        // Assert
        mappedManager.ShouldNotBeNull();
        ShouldBeStringTestExtensions.ShouldBe(mappedManager.FirstName, "Bob");
        ShouldBeStringTestExtensions.ShouldBe(mappedManager.LastName, "Wilson");
        mappedManager.TeamName.ShouldBe(originalManager.TeamName);
        mappedManager.TeamSize.ShouldBe(originalManager.TeamSize);

        // Excluded properties should have default values
        mappedManager.Budget.ShouldBe(0);
        ShouldBeTestExtensions.ShouldBe<decimal>(mappedManager.Salary, 0);
    }

    #endregion

    #region Record Tests

    [Fact]
    public void ToSource_ShouldMapProductRecord_WithBasicProperties()
    {
        // Arrange
        var originalProduct = TestDataFactory.CreateProduct("Test Product", 49.99m);

        var productDto = originalProduct.ToTarget<Product, ProductDto>();

        // Act
        var mappedProduct = productDto.ToSource<Product>();

        // Assert
        mappedProduct.ShouldNotBeNull();
        mappedProduct.Id.ShouldBe(originalProduct.Id);
        mappedProduct.Name.ShouldBe("Test Product");
        mappedProduct.Description.ShouldBe(originalProduct.Description);
        mappedProduct.Price.ShouldBe(49.99m);
        mappedProduct.CategoryId.ShouldBe(originalProduct.CategoryId);
        mappedProduct.IsAvailable.ShouldBe(originalProduct.IsAvailable);

        // Excluded property should have default value
        mappedProduct.InternalNotes.ShouldBeEmpty();
    }

    [Fact]
    public void ToSource_ShouldHandleRecord_WithPositionalConstructor()
    {
        // Arrange
        var originalClassicUser = TestDataFactory.CreateClassicUser("Alice", "Wonder");
        var classicUserDto = originalClassicUser.ToTarget<ClassicUser, ClassicUserDto>();

        // Act
        var mappedClassicUser = classicUserDto.ToSource<ClassicUser>();

        // Assert
        mappedClassicUser.ShouldNotBeNull();
        mappedClassicUser.Id.ShouldBe(originalClassicUser.Id);
        mappedClassicUser.FirstName.ShouldBe("Alice");
        mappedClassicUser.LastName.ShouldBe("Wonder");
        mappedClassicUser.Email.ShouldBe(originalClassicUser.Email);
    }

    [Fact]
    public void ToSource_ShouldHandleModernRecord_WithGettersAndInitializers()
    {
        // Arrange
        var originalModernUser = TestDataFactory.CreateModernUser("Alice", "Wonder");
        var modernUserDto = originalModernUser.ToTarget<ModernUser, ModernUserDto>();

        // Act
        var mappedModernUser = modernUserDto.ToSource<ModernUser>();

        // Assert
        mappedModernUser.ShouldNotBeNull();
        mappedModernUser.Id.ShouldBe(originalModernUser.Id);
        mappedModernUser.FirstName.ShouldBe("Alice");
        mappedModernUser.LastName.ShouldBe("Wonder");
        mappedModernUser.Email.ShouldBe(originalModernUser.Email);
        mappedModernUser.CreatedAt.ShouldBe(originalModernUser.CreatedAt);

        // Excluded properties should have default values
        mappedModernUser.Bio.ShouldBeNull();
        mappedModernUser.PasswordHash.ShouldBeNull();
    }

    [Fact]
    public void ToSource_ShouldHandleRecordEquality_WithValueSemantics()
    {
        // Arrange
        var product = TestDataFactory.CreateProduct("Equality Test", 10.99m);
        var dto1 = product.ToTarget<Product, ProductDto>();
        var dto2 = product.ToTarget<Product, ProductDto>();

        // Act
        var mapped1 = dto1.ToSource<Product>();
        var mapped2 = dto2.ToSource<Product>();

        // Assert
        dto1.ShouldBe(dto2, "Records should have value equality");
        mapped1.Id.ShouldBe(mapped2.Id);
        mapped1.Name.ShouldBe(mapped2.Name);
        mapped1.Price.ShouldBe(mapped2.Price);
    }

    #endregion

    #region Enum Handling Tests

    [Fact]
    public void ToSource_ShouldPreserveEnumValues_WhenMappingUserWithEnum()
    {
        // Arrange
        var originalUser = TestDataFactory.CreateUserWithEnum("Enum User");
        var userDto = originalUser.ToTarget<UserWithEnum, UserWithEnumDto>();

        // Act
        var mappedUser = userDto.ToSource<UserWithEnum>();

        // Assert
        mappedUser.ShouldNotBeNull();
        mappedUser.Id.ShouldBe(originalUser.Id);
        mappedUser.Name.ShouldBe("Enum User");
        mappedUser.Email.ShouldBe(originalUser.Email);
        mappedUser.Status.ShouldBe(UserStatus.Active);
    }

    [Theory]
    [InlineData(UserStatus.Active)]
    [InlineData(UserStatus.Inactive)]
    [InlineData(UserStatus.Pending)]
    [InlineData(UserStatus.Suspended)]
    public void ToSource_ShouldHandleAllEnumValues_Correctly(UserStatus status)
    {
        // Arrange
        var originalUser = TestDataFactory.CreateUserWithEnum("Test User", status);
        var userDto = originalUser.ToTarget<UserWithEnum, UserWithEnumDto>();

        // Act
        var mappedUser = userDto.ToSource<UserWithEnum>();

        // Assert
        mappedUser.Status.ShouldBe(status);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ToSource_ShouldHandleDefaultValues_WhenDtoHasMinimalData()
    {
        // Arrange
        var userDto = new UserDto
        {
            Id = 999,
            FirstName = "Minimal",
            LastName = "User",
            Email = "minimal@test.com",
            DateOfBirth = new DateTime(1985, 1, 1),
            IsActive = true,
            LastLoginAt = null
        };

        // Act
        var mappedUser = userDto.ToSource<User>();

        // Assert
        mappedUser.ShouldNotBeNull();
        mappedUser.Id.ShouldBe(999);
        mappedUser.FirstName.ShouldBe("Minimal");
        mappedUser.LastName.ShouldBe("User");
        mappedUser.Email.ShouldBe("minimal@test.com");
        mappedUser.Password.ShouldBeEmpty(); // Default value for excluded property
        mappedUser.CreatedAt.ShouldBe(default(DateTime)); // Default value for excluded property
    }

    [Fact]
    public void ToSource_ShouldRoundTrip_PreservingIncludedProperties()
    {
        // Arrange
        var originalUser = TestDataFactory.CreateUser("Round", "Trip", "round@trip.com");

        // Act - Round trip: User -> UserDto -> User
        var userDto = originalUser.ToTarget<User, UserDto>();
        var roundTripUser = userDto.ToSource<User>();

        // Assert - Included properties should match
        roundTripUser.Id.ShouldBe(originalUser.Id);
        roundTripUser.FirstName.ShouldBe(originalUser.FirstName);
        roundTripUser.LastName.ShouldBe(originalUser.LastName);
        roundTripUser.Email.ShouldBe(originalUser.Email);
        roundTripUser.DateOfBirth.ShouldBe(originalUser.DateOfBirth);
        roundTripUser.IsActive.ShouldBe(originalUser.IsActive);
        roundTripUser.LastLoginAt.ShouldBe(originalUser.LastLoginAt);

        // Excluded properties should be defaults (data loss is expected)
        roundTripUser.Password.ShouldBeEmpty();
        roundTripUser.CreatedAt.ShouldBe(default(DateTime));
    }

    [Fact]
    public void ToSource_ShouldNotBeNull_ForValidDtoInput()
    {
        // Arrange
        var userDto = new UserDto
        {
            Id = 123,
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            DateOfBirth = DateTime.Now.AddYears(-25),
            IsActive = true,
            LastLoginAt = DateTime.Now.AddHours(-1)
        };

        // Act
        var result = userDto.ToSource<User>();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<User>();
    }

    [Fact]
    public void ToSource_ShouldPreserveDecimalPrecision_InProductMapping()
    {
        // Arrange
        var originalProduct = TestDataFactory.CreateProduct("Precision Test", 123.456789m);
        var productDto = originalProduct.ToTarget<Product, ProductDto>();

        // Act
        var mappedProduct = productDto.ToSource<Product>();

        // Assert
        mappedProduct.Price.ShouldBe(123.456789m);
    }

    [Fact]
    public void ToSource_ShouldHandleDateTimePrecision_Correctly()
    {
        // Arrange
        var specificDate = new DateTime(2024, 3, 15, 14, 30, 45, 123);
        var user = TestDataFactory.CreateUser(dateOfBirth: specificDate);
        var userDto = user.ToTarget<User, UserDto>();

        // Act
        var mappedUser = userDto.ToSource<User>();

        // Assert
        mappedUser.DateOfBirth.ShouldBe(specificDate);
    }

    #endregion
}
