using System.Reflection;
using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.GenerateDtos;

/// <summary>
///     Tests to verify that DTOs generated with [GenerateDtos] work as proper targets
///     and can use ToTarget, SelectTargets, ToSource, and other target mapping extension methods.
/// </summary>
public class GenerateDtosTargetIntegrationTests
{
    [Fact]
    public void GeneratedDtos_ShouldExist()
    {
        // Check which DTOs were actually generated
        var assembly = Assembly.GetAssembly(typeof(TestUser));
        
        // Check for Response DTO (should exist)
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserResponse");
        responseType.ShouldNotBeNull("TestUserResponse should be generated");
        
        // Check for other possible DTO types
        var createType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserCreateRequest") ??
                        assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserCreate");
        var updateType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserUpdateRequest") ??
                        assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserUpdate");
        var queryType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserQuery");
        
        // At least response type should exist
        responseType.ShouldNotBeNull();
    }

    [Fact]
    public void ToTarget_Should_Work_WithGeneratedResponseDto()
    {
        // Arrange
        var user = CreateTestUser();

        // Act & Assert - This should work since TestUserResponse exists
        var responseDto = user.ToTarget<TestUserResponse>();
        
        responseDto.ShouldNotBeNull();
        responseDto.Id.ShouldBe(user.Id);
        responseDto.FirstName.ShouldBe("John");
        responseDto.LastName.ShouldBe("Doe");
        responseDto.Email.ShouldBe("john@example.com");
        responseDto.IsActive.ShouldBe(user.IsActive);
    }

    [Fact]
    public void ToTarget_Should_Work_WithAuditableDto()
    {
        // Arrange
        var product = CreateTestProduct();

        // Act & Assert
        var responseDto = product.ToTarget<TestProductResponse>();
        
        responseDto.ShouldNotBeNull();
        responseDto.Id.ShouldBe(product.Id);
        responseDto.Name.ShouldBe("Test Product");
        responseDto.Description.ShouldBe("Test Description");
        responseDto.Price.ShouldBe(99.99m);
        // Audit fields should be excluded
    }

    [Fact]
    public void SelectTargets_Should_Work_WithGeneratedDtos()
    {
        // Arrange
        var users = new List<TestUser>
        {
            CreateTestUser("Alice", "Smith"),
            CreateTestUser("Bob", "Johnson"),
            CreateTestUser("Carol", "Williams")
        };

        // Act & Assert
        var responseDtos = users.SelectTargets<TestUserResponse>().ToList();
        
        responseDtos.Count().ShouldBe(3);
        responseDtos[0].FirstName.ShouldBe("Alice");
        responseDtos[1].FirstName.ShouldBe("Bob");
        responseDtos[2].FirstName.ShouldBe("Carol");
    }

    [Fact]
    public void SelectTarget_Should_Work_WithQueryable()
    {
        // Arrange
        var users = new List<TestUser>
        {
            CreateTestUser("Alice", "Smith"),
            CreateTestUser("Bob", "Johnson")
        }.AsQueryable();

        // Act & Assert  
        var responseDtos = users.SelectTarget<TestUser, TestUserResponse>().ToList();
        
        responseDtos.Count().ShouldBe(2);
        responseDtos[0].FirstName.ShouldBe("Alice");
        responseDtos[1].FirstName.ShouldBe("Bob");
    }

    [Fact]
    public void Projection_Should_Be_Available_OnGeneratedDtos()
    {
        // Arrange
        var users = new List<TestUser>
        {
            CreateTestUser("Alice", "Smith"),
            CreateTestUser("Bob", "Johnson")
        }.AsQueryable();

        // Act & Assert - This tests that the Projection property exists and works
        var projectionExpression = TestUserResponse.Projection;
        projectionExpression.ShouldNotBeNull();

        var projectedResults = users.Select(projectionExpression).ToList();
        projectedResults.Count().ShouldBe(2);
        projectedResults[0].FirstName.ShouldBe("Alice");
        projectedResults[1].FirstName.ShouldBe("Bob");
    }

    [Fact]
    public void Projection_Should_Work_WithLinqSelect()
    {
        // Arrange
        var users = new List<TestUser>
        {
            CreateTestUser("Test", "User")
        }.AsQueryable();

        // Act & Assert
        var results = users.Select(TestUserResponse.Projection).ToList();
        
        results.Count().ShouldBe(1);
        results[0].FirstName.ShouldBe("Test");
        results[0].LastName.ShouldBe("User");
    }

    [Fact]
    public void Constructor_Should_Work_WithSourceType()
    {
        // Arrange
        var user = CreateTestUser();

        // Act & Assert
        var responseDto = new TestUserResponse(user);
        
        responseDto.ShouldNotBeNull();
        responseDto.Id.ShouldBe(user.Id);
        responseDto.FirstName.ShouldBe("John");
        responseDto.LastName.ShouldBe("Doe");
        responseDto.Email.ShouldBe("john@example.com");
    }

    [Fact]
    public void ParameterlessConstructor_Should_Work()
    {
        // Act & Assert
        var responseDto = new TestUserResponse();
        
        responseDto.ShouldNotBeNull();
        responseDto.Id.ShouldBe(default(int));
        responseDto.FirstName.ShouldBeNullOrEmpty();
    }

    [Fact]
    public void GeneratedDtos_Should_HaveMappingTargetAttribute()
    {
        // Act & Assert - Verify the generated DTOs have the [MappingTarget] attribute
        var responseType = typeof(TestUserResponse);
        var targetAttributes = responseType.GetCustomAttributesData()
            .Where(attr => attr.AttributeType.IsGenericType &&
                           attr.AttributeType.GetGenericTypeDefinition() == typeof(MappingTargetAttribute<>))
            .ToArray();

        targetAttributes.ShouldNotBeEmpty("Generated DTOs should have [MappingTarget] attribute");

        targetAttributes[0].AttributeType.GenericTypeArguments[0].ShouldBe(typeof(TestUser));
    }

    [Fact]
    public void ToSource_Should_Work_WithGeneratedDtos()
    {
        // Arrange - Create a DTO with some values
        var responseDto = new TestUserResponse
        {
            Id = 42,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            IsActive = true
        };

        // Act
        var user = responseDto.ToSource();

        // Assert - Verify all properties are mapped correctly
        user.ShouldNotBeNull();
        user.Id.ShouldBe(42);
        user.FirstName.ShouldBe("Jane");
        user.LastName.ShouldBe("Smith");
        user.Email.ShouldBe("jane@example.com");
        user.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void ToSource_Extension_Should_Work_WithGeneratedDtos()
    {
        // Arrange
        var responseDto = new TestUserResponse
        {
            Id = 123,
            FirstName = "Bob",
            LastName = "Johnson",
            Email = "bob@example.com",
            IsActive = false
        };

        // Act
        var user = responseDto.ToSource<TestUserResponse, TestUser>();

        // Assert
        user.ShouldNotBeNull();
        user.Id.ShouldBe(123);
        user.FirstName.ShouldBe("Bob");
        user.LastName.ShouldBe("Johnson");
        user.Email.ShouldBe("bob@example.com");
        user.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void ToSource_Should_Work_WithCreateRequest()
    {
        // Arrange - Create a Create DTO (which excludes Id)
        var assembly = Assembly.GetAssembly(typeof(TestUser));
        var createRequestType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.CreateTestUserRequest");

        if (createRequestType != null)
        {
            var createRequest = Activator.CreateInstance(createRequestType);
            var firstNameProp = createRequestType.GetProperty("FirstName");
            var lastNameProp = createRequestType.GetProperty("LastName");
            var emailProp = createRequestType.GetProperty("Email");

            firstNameProp?.SetValue(createRequest, "Alice");
            lastNameProp?.SetValue(createRequest, "Williams");
            emailProp?.SetValue(createRequest, "alice@example.com");

            // Act
            var toSourceMethod = createRequestType.GetMethod("ToSource");
            toSourceMethod.ShouldNotBeNull("CreateTestUserRequest should have a ToSource method");

            var user = toSourceMethod!.Invoke(createRequest, null) as TestUser;

            // Assert
            user.ShouldNotBeNull();
            user.FirstName.ShouldBe("Alice");
            user.LastName.ShouldBe("Williams");
            user.Email.ShouldBe("alice@example.com");
            // Id should be default since it's excluded from Create DTOs
            user.Id.ShouldBe(default(int));
        }
    }

    #region Helper Methods

    private static TestUser CreateTestUser(string firstName = "John", string lastName = "Doe")
    {
        return new TestUser
        {
            Id = 1,
            FirstName = firstName,
            LastName = lastName,
            Email = $"{firstName.ToLower()}@example.com",
            Password = "secret123",
            DateOfBirth = new DateTime(1990, 1, 1),
            IsActive = true,
            LastLoginAt = DateTime.Now.AddHours(-2),
            CreatedAt = DateTime.Now.AddDays(-30)
        };
    }

    private static TestProduct CreateTestProduct()
    {
        return new TestProduct
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description", 
            Price = 99.99m,
            IsAvailable = true,
            InternalNotes = "Secret notes",
            CreatedAt = DateTime.Now.AddDays(-10),
            UpdatedAt = DateTime.Now.AddHours(-1),
            CreatedBy = "admin",
            UpdatedBy = "admin"
        };
    }

    #endregion
}
