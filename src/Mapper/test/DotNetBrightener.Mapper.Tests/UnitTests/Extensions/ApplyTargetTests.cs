using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Tests.TestModels;
using DotNetBrightener.Mapper.Tests.Utilities;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Extensions;

public class ApplyTargetTests
{
    [Fact]
    public void ApplyTarget_ShouldUpdateChangedProperties_WhenTargetHasDifferentValues()
    {
        // Arrange
        var user = TestDataFactory.CreateUser("John", "Doe", "john@example.com");
        var target = new UserDto
        {
            Id = user.Id,
            FirstName = "Jane",  // Changed
            LastName = "Doe",    // Unchanged
            Email = "jane@example.com",  // Changed
            IsActive = user.IsActive,     // Unchanged
            DateOfBirth = user.DateOfBirth,  // Unchanged
            LastLoginAt = user.LastLoginAt   // Unchanged
        };

        // Act
        user.ApplyTarget<User, UserDto>(target);

        // Assert
        user.FirstName.ShouldBe("Jane");
        user.Email.ShouldBe("jane@example.com");
        user.LastName.ShouldBe("Doe");
        user.IsActive.ShouldBe(target.IsActive);
    }

    [Fact]
    public void ApplyTarget_ShouldNotUpdateUnchangedProperties()
    {
        // Arrange
        var user = TestDataFactory.CreateUser("John", "Doe", "john@example.com");
        var originalLastName = user.LastName;
        var originalEmail = user.Email;

        var target = new UserDto
        {
            Id = user.Id,
            FirstName = "Jane",  // Changed
            LastName = originalLastName,  // Unchanged
            Email = originalEmail,  // Unchanged
            IsActive = user.IsActive,
            DateOfBirth = user.DateOfBirth,
            LastLoginAt = user.LastLoginAt
        };

        // Act
        user.ApplyTarget<User, UserDto>(target);

        // Assert
        user.FirstName.ShouldBe("Jane");
        user.LastName.ShouldBe(originalLastName);
        user.Email.ShouldBe(originalEmail);
    }

    [Fact]
    public void ApplyTarget_ShouldHandleNullableProperties()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        user.LastLoginAt = DateTime.Now;

        var target = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            IsActive = user.IsActive,
            DateOfBirth = user.DateOfBirth,
            LastLoginAt = null  // Changed to null
        };

        // Act
        user.ApplyTarget<User, UserDto>(target);

        // Assert
        user.LastLoginAt.ShouldBeNull();
    }

    [Fact]
    public void ApplyTarget_ShouldReturnSourceInstance_ForFluentChaining()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var target = new UserDto
        {
            Id = user.Id,
            FirstName = "Updated",
            LastName = user.LastName,
            Email = user.Email,
            IsActive = user.IsActive,
            DateOfBirth = user.DateOfBirth,
            LastLoginAt = user.LastLoginAt
        };

        // Act
        var result = user.ApplyTarget<User, UserDto>(target);

        // Assert
        result.ShouldBeSameAs(user);
        result.FirstName.ShouldBe("Updated");
    }

    [Fact]
    public void ApplyTarget_WithInferredType_ShouldUpdateProperties()
    {
        // Arrange
        var user = TestDataFactory.CreateUser("John", "Doe", "john@example.com");
        var target = new UserDto
        {
            Id = user.Id,
            FirstName = "Jane",
            LastName = "Smith",
            Email = user.Email,
            IsActive = user.IsActive,
            DateOfBirth = user.DateOfBirth,
            LastLoginAt = user.LastLoginAt
        };

        // Act
        user.ApplyTarget(target);

        // Assert
        user.FirstName.ShouldBe("Jane");
        user.LastName.ShouldBe("Smith");
        user.Email.ShouldBe(target.Email);
    }

    [Fact]
    public void ApplyTargetWithChanges_ShouldReturnChangedPropertyNames()
    {
        // Arrange
        var user = TestDataFactory.CreateUser("John", "Doe", "john@example.com");
        var target = user.ToTarget<User, UserDto>();

        // Modify specific properties
        target.FirstName = "Jane";  // Changed
        target.Email = "jane@example.com";  // Changed
        // LastName unchanged

        // Act
        var result = user.ApplyTargetWithChanges<User, UserDto>(target);

        // Assert
        result.Source.ShouldBeSameAs(user);
        result.HasChanges.ShouldBeTrue();
        result.ChangedProperties.ShouldContain("FirstName");
        result.ChangedProperties.ShouldContain("Email");
        result.ChangedProperties.ShouldNotContain("LastName");
        result.ChangedProperties.ShouldNotContain("Id");
        result.ChangedProperties.Count().ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void ApplyTargetWithChanges_ShouldReturnNoChanges_WhenAllPropertiesMatch()
    {
        // Arrange
        var user = TestDataFactory.CreateUser("John", "Doe", "john@example.com");
        var target = user.ToTarget<User, UserDto>();

        // Act
        var result = user.ApplyTargetWithChanges<User, UserDto>(target);

        // Assert
        result.HasChanges.ShouldBeFalse();
        result.ChangedProperties.ShouldBeEmpty();
    }

    [Fact]
    public void ApplyTarget_ShouldThrowArgumentNullException_WhenSourceIsNull()
    {
        // Arrange
        User? user = null;
        var target = new UserDto();

        // Act & Assert
        var act = () => user!.ApplyTarget<User, UserDto>(target);
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("source");
    }

    [Fact]
    public void ApplyTarget_ShouldThrowArgumentNullException_WhenTargetIsNull()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        UserDto? target = null;

        // Act & Assert
        var act = () => user.ApplyTarget<User, UserDto>(target!);
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("target");
    }

    [Fact]
    public void ApplyTarget_ShouldHandleBooleanChanges()
    {
        // Arrange
        var user = TestDataFactory.CreateUser(isActive: true);
        var target = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            IsActive = false,  // Changed
            DateOfBirth = user.DateOfBirth,
            LastLoginAt = user.LastLoginAt
        };

        // Act
        user.ApplyTarget<User, UserDto>(target);

        // Assert
        user.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void ApplyTarget_ShouldOnlyUpdatePropertiesExistingInBoth()
    {
        // Arrange
        var user = TestDataFactory.CreateUser("John", "Doe", "john@example.com");
        var originalPassword = user.Password;
        var originalCreatedAt = user.CreatedAt;

        var target = new UserDto
        {
            Id = user.Id,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            IsActive = false,
            DateOfBirth = user.DateOfBirth,
            LastLoginAt = user.LastLoginAt
        };

        // Act
        user.ApplyTarget<User, UserDto>(target);

        // Assert
        user.FirstName.ShouldBe("Jane");
        user.LastName.ShouldBe("Smith");
        user.Email.ShouldBe("jane@example.com");
        user.IsActive.ShouldBeFalse();

        // Properties not in the target should remain unchanged
        user.Password.ShouldBe(originalPassword);
        user.CreatedAt.ShouldBe(originalCreatedAt);
    }
}
