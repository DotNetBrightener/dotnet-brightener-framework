using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Tests.TestModels;
using DotNetBrightener.Mapper.Tests.Utilities;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Features;

public class EnumHandlingTests
{
    [Fact]
    public void ToTarget_ShouldMapEnumProperties_Correctly()
    {
        // Arrange
        var user = TestDataFactory.CreateUserWithEnum("Active User", UserStatus.Active);

        // Act
        var dto = user.ToTarget<UserWithEnum, UserWithEnumDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(user.Id);
        dto.Name.ShouldBe("Active User");
        dto.Status.ShouldBe(UserStatus.Active);
        dto.Email.ShouldBe(user.Email);
    }

    [Theory]
    [InlineData(UserStatus.Active)]
    [InlineData(UserStatus.Inactive)]
    [InlineData(UserStatus.Pending)]
    [InlineData(UserStatus.Suspended)]
    public void ToTarget_ShouldHandleAllEnumValues_Correctly(UserStatus status)
    {
        // Arrange
        var user = TestDataFactory.CreateUserWithEnum($"User {status}", status);

        // Act
        var dto = user.ToTarget<UserWithEnum, UserWithEnumDto>();

        // Assert
        dto.Status.ShouldBe(status);
        dto.Name.ShouldBe($"User {status}");
    }

    [Fact]
    public void ToTarget_ShouldPreserveEnumTypeInformation()
    {
        // Arrange
        var user = TestDataFactory.CreateUserWithEnum(status: UserStatus.Pending);

        // Act
        var dto = user.ToTarget<UserWithEnum, UserWithEnumDto>();

        // Assert
        dto.Status.GetType().ShouldBe(typeof(UserStatus));
        dto.Status.GetType().ShouldBe(typeof(UserStatus));
    }

    [Fact]
    public void ToTarget_ShouldAllowEnumComparison_AfterMapping()
    {
        // Arrange
        var activeUser = TestDataFactory.CreateUserWithEnum("Active", UserStatus.Active);
        var inactiveUser = TestDataFactory.CreateUserWithEnum("Inactive", UserStatus.Inactive);

        // Act
        var activeDto = activeUser.ToTarget<UserWithEnum, UserWithEnumDto>();
        var inactiveDto = inactiveUser.ToTarget<UserWithEnum, UserWithEnumDto>();

        // Assert
        activeDto.Status.ShouldNotBe(inactiveDto.Status);
        (activeDto.Status == UserStatus.Active).ShouldBeTrue();
        (inactiveDto.Status == UserStatus.Inactive).ShouldBeTrue();
    }

    [Fact]
    public void ToTarget_ShouldHandleEnumToStringConversion_IfNeeded()
    {
        // Arrange
        var user = TestDataFactory.CreateUserWithEnum("String Test", UserStatus.Suspended);

        // Act
        var dto = user.ToTarget<UserWithEnum, UserWithEnumDto>();

        // Assert
        dto.Status.ToString().ShouldBe("Suspended");
        Enum.GetName(typeof(UserStatus), dto.Status).ShouldBe("Suspended");
    }

    [Fact]
    public void ToTarget_ShouldMaintainEnumOrdinalValues()
    {
        // Arrange
        var user = TestDataFactory.CreateUserWithEnum(status: UserStatus.Pending);

        // Act
        var dto = user.ToTarget<UserWithEnum, UserWithEnumDto>();

        // Assert
        ((int)dto.Status).ShouldBe((int)UserStatus.Pending);
        ((int)dto.Status).ShouldBe(2);
    }

    [Fact]
    public void ToTarget_ShouldHandleMultipleUsersWithDifferentEnumValues()
    {
        // Arrange
        var users = new List<UserWithEnum>
        {
            TestDataFactory.CreateUserWithEnum("User 1", UserStatus.Active),
            TestDataFactory.CreateUserWithEnum("User 2", UserStatus.Inactive),
            TestDataFactory.CreateUserWithEnum("User 3", UserStatus.Pending),
            TestDataFactory.CreateUserWithEnum("User 4", UserStatus.Suspended)
        };

        // Act
        var dtos = users.Select(u => u.ToTarget<UserWithEnum, UserWithEnumDto>()).ToList();

        // Assert
        dtos.Count().ShouldBe(4);
        dtos[0].Status.ShouldBe(UserStatus.Active);
        dtos[1].Status.ShouldBe(UserStatus.Inactive);
        dtos[2].Status.ShouldBe(UserStatus.Pending);
        dtos[3].Status.ShouldBe(UserStatus.Suspended);
    }

    [Fact]
    public void UserStatusEnum_ShouldHaveExpectedValues()
    {
        // Assert - Verify the enum values are as expected
        ((int)UserStatus.Active).ShouldBe(0);
        ((int)UserStatus.Inactive).ShouldBe(1);
        ((int)UserStatus.Pending).ShouldBe(2);
        ((int)UserStatus.Suspended).ShouldBe(3);
    }

    [Fact]
    public void ToTarget_ShouldAllowEnumBasedFiltering_AfterMapping()
    {
        // Arrange
        var users = new List<UserWithEnum>
        {
            TestDataFactory.CreateUserWithEnum("Active 1", UserStatus.Active),
            TestDataFactory.CreateUserWithEnum("Inactive 1", UserStatus.Inactive),
            TestDataFactory.CreateUserWithEnum("Active 2", UserStatus.Active),
            TestDataFactory.CreateUserWithEnum("Pending 1", UserStatus.Pending)
        };

        // Act
        var dtos = users.Select(u => u.ToTarget<UserWithEnum, UserWithEnumDto>()).ToList();
        var activeUsers = dtos.Where(dto => dto.Status == UserStatus.Active).ToList();

        // Assert
        activeUsers.Count().ShouldBe(2);
        activeUsers.All(u => u.Status == UserStatus.Active).ShouldBeTrue();
        activeUsers.Select(u => u.Name).ShouldBe(["Active 1", "Active 2"]);
    }
}
