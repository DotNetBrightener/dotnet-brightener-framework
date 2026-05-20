using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Tests.TestModels;
using DotNetBrightener.Mapper.Tests.Utilities;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

public class CustomMappingTests
{
    [Fact]
    public void ToTarget_ShouldApplyCustomMapping_WhenConfigurationIsProvided()
    {
        // Arrange
        var user = TestDataFactory.CreateUser("John", "Doe", dateOfBirth: new DateTime(1990, 5, 15));

        // Act
        var dto = user.ToTarget<User, UserDtoWithMapping>();

        // Assert
        dto.ShouldNotBeNull();
        dto.FullName.ShouldBe("John Doe", "Custom mapping should combine first and last name");
        dto.Age.ShouldBeGreaterThan(30, "Custom mapping should calculate age from birth date");
    }

    [Fact]
    public void ToTarget_ShouldCalculateAge_BasedOnCurrentDate()
    {
        // Arrange
        var birthDate = DateTime.Today.AddYears(-25);
        var user = TestDataFactory.CreateUser("Jane", "Smith", dateOfBirth: birthDate);

        // Act
        var dto = user.ToTarget<User, UserDtoWithMapping>();

        // Assert
        dto.Age.ShouldBe(25, "Age should be calculated from birth date");
    }

    [Fact]
    public void ToTarget_ShouldHandleBirthdayNotYetPassed_InCurrentYear()
    {
        // Arrange - birthday is 6 months from now, so it hasn't passed this year
        var today = DateTime.Today;
        var birthDate = today.AddMonths(6).AddYears(-30);
        var user = TestDataFactory.CreateUser("Future", "Birthday", dateOfBirth: birthDate);

        // Act
        var dto = user.ToTarget<User, UserDtoWithMapping>();

        // Assert - person turns 30 in 6 months, so currently 29
        dto.Age.ShouldBe(29, "Age should be 29 if 30th birthday hasn't occurred this year yet");
    }

    [Fact]
    public void ToTarget_ShouldHandleBirthdayAlreadyPassed_InCurrentYear()
    {
        // Arrange - birthday was 6 months ago, so it has already passed this year
        var today = DateTime.Today;
        var birthDate = today.AddMonths(-6).AddYears(-30);
        var user = TestDataFactory.CreateUser("Past", "Birthday", dateOfBirth: birthDate);

        // Act
        var dto = user.ToTarget<User, UserDtoWithMapping>();

        // Assert - person turned 30 six months ago
        dto.Age.ShouldBe(30, "Age should be 30 if birthday has already occurred this year");
    }

    [Fact]
    public void ToTarget_ShouldCombineNamesCorrectly_WithDifferentInputs()
    {
        // Arrange
        var testCases = new[]
        {
            ("John", "Doe", "John Doe"),
            ("Mary", "Smith-Johnson", "Mary Smith-Johnson"),
            ("", "SingleName", " SingleName"),
            ("OnlyFirst", "", "OnlyFirst "),
            ("", "", " ")
        };

        foreach (var (firstName, lastName, expectedFullName) in testCases)
        {
            // Act
            var user = TestDataFactory.CreateUser(firstName, lastName);
            var dto = user.ToTarget<User, UserDtoWithMapping>();

            // Assert
            dto.FullName.ShouldBe(expectedFullName,
                $"FullName should be '{expectedFullName}' for '{firstName}' + '{lastName}'");
        }
    }

    [Fact]
    public void ToTarget_ShouldStillExcludeSpecifiedProperties_WhenUsingCustomMapping()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();

        // Act
        var dto = user.ToTarget<User, UserDtoWithMapping>();

        // Assert
        var dtoType = dto.GetType();
        dtoType.GetProperty("Password").ShouldBeNull("Password should still be excluded");
        dtoType.GetProperty("CreatedAt").ShouldBeNull("CreatedAt should still be excluded");
    }

    [Fact]
    public void ToTarget_ShouldIncludeStandardProperties_EvenWithCustomMapping()
    {
        // Arrange
        var user = TestDataFactory.CreateUser("Custom", "Mapping", "custom@example.com");

        // Act
        var dto = user.ToTarget<User, UserDtoWithMapping>();

        // Assert
        dto.Id.ShouldBe(user.Id);
        dto.FirstName.ShouldBe("Custom");
        dto.LastName.ShouldBe("Mapping");
        dto.Email.ShouldBe("custom@example.com");
        dto.IsActive.ShouldBe(user.IsActive);

        // Plus the custom mapped properties
        dto.FullName.ShouldBe("Custom Mapping");
        dto.Age.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void ToTarget_CustomMapping_ShouldWorkWithBoundaryAges()
    {
        // Arrange - Test with very young and very old ages
        var today = DateTime.Today;
        var veryYoung = TestDataFactory.CreateUser("Young", "Person", dateOfBirth: today.AddYears(-1));
        var veryOld = TestDataFactory.CreateUser("Old", "Person", dateOfBirth: today.AddYears(-100));

        // Act
        var youngDto = veryYoung.ToTarget<User, UserDtoWithMapping>();
        var oldDto = veryOld.ToTarget<User, UserDtoWithMapping>();

        // Assert
        youngDto.Age.ShouldBe(1);
        oldDto.Age.ShouldBe(100);
    }
}
