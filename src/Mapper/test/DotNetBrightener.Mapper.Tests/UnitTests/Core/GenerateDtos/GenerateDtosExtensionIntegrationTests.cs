using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.GenerateDtos;

/// <summary>
///     Tests for integration between GenerateDtos and target mapping extension methods.
///     Verifies that generated DTOs work seamlessly with ToTarget, SelectTargets, etc.
/// </summary>
public class GenerateDtosExtensionIntegrationTests
{
    [Fact]
    public void GeneratedResponseDto_ShouldWork_WithToTargetExtension()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var responseDto = user.ToTarget<TestUserResponse>();

        // Assert
        responseDto.ShouldNotBeNull();
        responseDto.Id.ShouldBe(user.Id);
        responseDto.FirstName.ShouldBe(user.FirstName);
        responseDto.LastName.ShouldBe(user.LastName);
        responseDto.Email.ShouldBe(user.Email);
        responseDto.IsActive.ShouldBe(user.IsActive);
        responseDto.DateOfBirth.ShouldBe(user.DateOfBirth);
        responseDto.LastLoginAt.ShouldBe(user.LastLoginAt);
        responseDto.CreatedAt.ShouldBe(user.CreatedAt);
    }

    [Fact]
    public void GeneratedResponseDto_ShouldWork_WithSelectTargetsExtension()
    {
        // Arrange
        var users = new List<TestUser>
        {
            CreateTestUser(1, "Alice", "Johnson", "alice@test.com"),
            CreateTestUser(2, "Bob", "Smith", "bob@test.com"),
            CreateTestUser(3, "Charlie", "Brown", "charlie@test.com")
        };

        // Act
        var responseDtos = users.SelectTargets<TestUserResponse>().ToList();

        // Assert
        responseDtos.Count().ShouldBe(3);
        responseDtos[0].FirstName.ShouldBe("Alice");
        responseDtos[0].LastName.ShouldBe("Johnson");
        responseDtos[1].FirstName.ShouldBe("Bob");
        responseDtos[1].LastName.ShouldBe("Smith");
        responseDtos[2].FirstName.ShouldBe("Charlie");
        responseDtos[2].LastName.ShouldBe("Brown");
    }

    [Fact]
    public void GeneratedResponseDto_ShouldWork_WithTypedSelectTargets()
    {
        // Arrange
        var users = new List<TestUser>
        {
            CreateTestUser(1, "Test1", "User1"),
            CreateTestUser(2, "Test2", "User2")
        };

        // Act - Using typed version for better performance
        var responseDtos = users.SelectTargets<TestUser, TestUserResponse>().ToList();

        // Assert
        responseDtos.Count().ShouldBe(2);
        responseDtos[0].FirstName.ShouldBe("Test1");
        responseDtos[1].FirstName.ShouldBe("Test2");
    }

    [Fact]
    public void GeneratedAuditableDto_ShouldWork_WithTargetExtensions()
    {
        // Arrange
        var product = CreateTestProduct();

        // Act - Test with auditable DTO (should exclude audit fields)
        var responseDto = product.ToTarget<TestProductResponse>();

        // Assert
        responseDto.ShouldNotBeNull();
        responseDto.Id.ShouldBe(product.Id);
        responseDto.Name.ShouldBe(product.Name);
        responseDto.Description.ShouldBe(product.Description);
        responseDto.Price.ShouldBe(product.Price);
        responseDto.IsAvailable.ShouldBe(product.IsAvailable);
        
        // Verify audit fields are not accessible (they should be excluded)
        var responseType = typeof(TestProductResponse);
        responseType.GetProperty("CreatedAt").ShouldBeNull("Audit fields should be excluded");
        responseType.GetProperty("CreatedBy").ShouldBeNull("Audit fields should be excluded");
        responseType.GetProperty("UpdatedAt").ShouldBeNull("Audit fields should be excluded");
        responseType.GetProperty("UpdatedBy").ShouldBeNull("Audit fields should be excluded");
    }

    [Fact]
    public void GeneratedDtos_ShouldWork_InLinqQueryScenarios()
    {
        // Arrange
        var users = new List<TestUser>
        {
            CreateTestUser(1, "Active", "User1", email: "active1@test.com", isActive: true),
            CreateTestUser(2, "Inactive", "User2", email: "inactive2@test.com", isActive: false),
            CreateTestUser(3, "Active", "User3", email: "active3@test.com", isActive: true)
        };

        // Act - Complex LINQ scenario with generated DTOs
        var activeUserDtos = users
            .Where(u => u.IsActive)
            .SelectTargets<TestUserResponse>()
            .Where(dto => dto.FirstName.StartsWith("Active"))
            .OrderBy(dto => dto.LastName)
            .ToList();

        // Assert
        activeUserDtos.Count().ShouldBe(2);
        activeUserDtos[0].LastName.ShouldBe("User1");
        activeUserDtos[1].LastName.ShouldBe("User3");
        activeUserDtos.All(dto => dto.IsActive).ShouldBeTrue();
    }

    [Fact]
    public void GeneratedDtos_ShouldWork_WithProjectionProperty()
    {
        // Arrange
        var users = new List<TestUser>
        {
            CreateTestUser(1, "Projection", "Test1"),
            CreateTestUser(2, "Projection", "Test2")
        }.AsQueryable();

        // Act - Using static Projection property
        var results = users.Select(TestUserResponse.Projection).ToList();

        // Assert
        results.Count().ShouldBe(2);
        results.ShouldAllBe(x => x is TestUserResponse);
        results[0].FirstName.ShouldBe("Projection");
        results[0].LastName.ShouldBe("Test1");
        results[1].FirstName.ShouldBe("Projection");
        results[1].LastName.ShouldBe("Test2");
    }

    [Fact]
    public void GeneratedDtos_ShouldWork_WithAsyncExtensions()
    {
        // Arrange
        var users = new List<TestUser>
        {
            CreateTestUser(1, "Async", "Test1"),
            CreateTestUser(2, "Async", "Test2")
        };

        // Act - Simulate async operation
        var task = Task.FromResult(users.SelectTargets<TestUserResponse>().ToList());
        var results = task.Result;

        // Assert
        results.Count().ShouldBe(2);
        results[0].FirstName.ShouldBe("Async");
        results[1].FirstName.ShouldBe("Async");
    }

    [Fact]
    public void GeneratedDtos_ShouldMaintain_TypeSafety()
    {
        // Arrange
        var user = CreateTestUser();

        // Act & Assert - Compile-time type safety
        TestUserResponse responseDto = user.ToTarget<TestUserResponse>();
        responseDto.ShouldNotBeNull();
        
        // Verify that we get strongly typed objects
        responseDto.ShouldBeOfType<TestUserResponse>();
        
        // Properties should be accessible with IntelliSense
        string firstName = responseDto.FirstName;
        int id = responseDto.Id;
        bool isActive = responseDto.IsActive;
        
        firstName.ShouldNotBeNull();
        id.ShouldBeGreaterThanOrEqualTo(0);
    }

    #region Helper Methods

    private static TestUser CreateTestUser(
        int id = 1, 
        string firstName = "Test", 
        string lastName = "User", 
        string email = null,
        bool isActive = true)
    {
        return new TestUser
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email ?? $"{firstName.ToLower()}.{lastName.ToLower()}@test.com",
            Password = "testpassword",
            DateOfBirth = new DateTime(1990, 1, 1),
            IsActive = isActive,
            LastLoginAt = DateTime.Now.AddHours(-1),
            CreatedAt = DateTime.Now.AddDays(-10)
        };
    }

    private static TestProduct CreateTestProduct()
    {
        return new TestProduct
        {
            Id = 1,
            Name = "Integration Test Product",
            Description = "Product for extension integration testing",
            Price = 99.99m,
            IsAvailable = true,
            InternalNotes = "Internal notes that should be excluded",
            CreatedAt = DateTime.Now.AddDays(-5),
            UpdatedAt = DateTime.Now.AddHours(-2),
            CreatedBy = "testuser",
            UpdatedBy = "testadmin"
        };
    }

    #endregion
}
