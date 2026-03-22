using System.Reflection;
using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.GenerateDtos;

public class GenerateDtosSimpleTest
{
    [Fact]
    public void GeneratedDtos_ShouldBeCreated()
    {
        // This simple test just verifies the generated types exist
        var responseType = typeof(TestUserResponse);
        responseType.ShouldNotBeNull();
        
        // Check what DTOs are actually available in this assembly
        var assembly = Assembly.GetAssembly(typeof(TestUser));
        var allTypes = assembly?.GetTypes()
            .Where(t => t.Name.StartsWith("TestUser"))
            .ToList() ??
                       [];

        // We know TestUserResponse exists based on compilation
        allTypes.ShouldContain(t => t.Name == "TestUserResponse");
    }
    
    [Fact]
    public void GeneratedDto_ShouldHaveMappingTargetAttribute()
    {
        // Check if the generated DTO has the MappingTarget attribute
        var responseType = typeof(TestUserResponse);
        var attributes = responseType.GetCustomAttributesData()
            .Where(attr => attr.AttributeType.IsGenericType &&
                           attr.AttributeType.GetGenericTypeDefinition() == typeof(MappingTargetAttribute<>))
            .ToArray();
        
        attributes.ShouldNotBeEmpty("Generated DTOs should have [MappingTarget] attribute");
    }
    
    [Fact]
    public void ToTarget_ShouldWork_WithBasicMapping()
    {
        // Arrange
        var user = new TestUser
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        // Act - Simple conversion test
        var responseDto = user.ToTarget<TestUserResponse>();
        
        // Assert
        responseDto.ShouldNotBeNull();
    }
    
    [Fact]
    public void AvailableGeneratedTypes_ShouldIncludeExpectedDtos()
    {
        // Get all types in the test assembly that start with TestUser
        var assembly = Assembly.GetAssembly(typeof(TestUser));
        var testUserTypes = assembly?.GetTypes()
            .Where(t => t.Name.StartsWith("TestUser"))
            .Select(t => t.Name)
            .OrderBy(name => name)
            .ToList() ??
                            [];
        
        // We should have at least TestUserResponse
        testUserTypes.ShouldContain("TestUserResponse");

        // The test entity specifies DtoTypes.All, so we should have multiple DTOs
        testUserTypes.Count.ShouldBeGreaterThan(1, "DtoTypes.All should generate multiple DTOs");
    }
}
