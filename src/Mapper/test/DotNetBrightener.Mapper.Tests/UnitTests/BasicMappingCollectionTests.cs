using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Tests.TestModels;
using DotNetBrightener.Mapper.Tests.Utilities;

namespace DotNetBrightener.Mapper.Tests.UnitTests;

public class BasicMappingCollectionTests
{
    [Fact]
    public void SelectTargets_ShouldMapBasicProperties_WhenMappingUserToDto()
    {
        // Arrange
        var users = TestDataFactory.CreateUsers();

        // Act
        var dtos = users.SelectTargets<User, UserDto>().ToList();

        // Assert
        dtos.ShouldNotBeNull();
        dtos.Count().ShouldBe(users.Count);
        dtos[0].FirstName.ShouldBe(users[0].FirstName);
        dtos[1].FirstName.ShouldBe(users[1].FirstName);
        dtos[2].FirstName.ShouldBe(users[2].FirstName);
        dtos[2].IsActive.ShouldBe(users[2].IsActive);
    }
    
    [Fact]
    public void SelectTargetsShorthand_ShouldMapBasicProperties_WhenMappingUserToDto()
    {
        // Arrange
        var users = TestDataFactory.CreateUsers();

        // Act
        var dtos = users.SelectTargets<UserDto>().ToList();

        // Assert
        dtos.ShouldNotBeNull();
        dtos.Count().ShouldBe(users.Count);
        dtos[0].FirstName.ShouldBe(users[0].FirstName);
        dtos[1].FirstName.ShouldBe(users[1].FirstName);
        dtos[2].FirstName.ShouldBe(users[2].FirstName);
        dtos[2].IsActive.ShouldBe(users[2].IsActive);
    }
}
