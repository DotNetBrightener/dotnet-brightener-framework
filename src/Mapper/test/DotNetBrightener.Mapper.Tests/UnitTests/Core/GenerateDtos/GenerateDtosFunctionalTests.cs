using System.Reflection;
using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.GenerateDtos;

/// <summary>
///     Tests for functional usage of generated DTOs with real data scenarios.
///     Verifies that generated DTOs work correctly in typical application patterns.
/// </summary>
public class GenerateDtosFunctionalTests
{
    [Fact]
    public void CreateDto_ShouldWork_ForUserCreationScenario()
    {
        // Arrange
        var assembly = Assembly.GetAssembly(typeof(TestUser));
        var createType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.CreateTestUserRequest");
        
        createType.ShouldNotBeNull();
        var createDto = Activator.CreateInstance(createType!);

        // Act - Simulate typical creation scenario
        var firstNameProp = createType.GetProperty("FirstName")!;
        var lastNameProp = createType.GetProperty("LastName")!;
        var emailProp = createType.GetProperty("Email")!;
        var passwordProp = createType.GetProperty("Password")!;
        var isActiveProp = createType.GetProperty("IsActive")!;

        firstNameProp.SetValue(createDto, "Jane");
        lastNameProp.SetValue(createDto, "Doe");
        emailProp.SetValue(createDto, "jane.doe@example.com");
        passwordProp.SetValue(createDto, "SecurePassword123");
        isActiveProp.SetValue(createDto, true);

        // Assert
        firstNameProp.GetValue(createDto).ShouldBe("Jane");
        lastNameProp.GetValue(createDto).ShouldBe("Doe");
        emailProp.GetValue(createDto).ShouldBe("jane.doe@example.com");
        passwordProp.GetValue(createDto).ShouldBe("SecurePassword123");
        isActiveProp.GetValue(createDto).ShouldBe(true);

        // Id should not exist on Create DTO
        var idProp = createType.GetProperty("Id");
        idProp.ShouldBeNull("Create DTOs should not have Id property");
    }

    [Fact]
    public void UpdateDto_ShouldWork_ForUserUpdateScenario()
    {
        // Arrange
        var assembly = Assembly.GetAssembly(typeof(TestUser));
        var updateType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.UpdateTestUserRequest");
        
        updateType.ShouldNotBeNull();
        var updateDto = Activator.CreateInstance(updateType!);

        // Act - Simulate typical update scenario
        var idProp = updateType.GetProperty("Id")!;
        var firstNameProp = updateType.GetProperty("FirstName")!;
        var emailProp = updateType.GetProperty("Email")!;

        idProp.SetValue(updateDto, 42);
        firstNameProp.SetValue(updateDto, "Updated Jane");
        emailProp.SetValue(updateDto, "jane.updated@example.com");

        // Assert
        idProp.GetValue(updateDto).ShouldBe(42);
        firstNameProp.GetValue(updateDto).ShouldBe("Updated Jane");
        emailProp.GetValue(updateDto).ShouldBe("jane.updated@example.com");
    }

    [Fact]
    public void ResponseDto_ShouldWork_ForApiResponseScenario()
    {
        // Arrange
        var user = CreateSampleTestUser();
        var assembly = Assembly.GetAssembly(typeof(TestUser));
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserResponse");
        
        responseType.ShouldNotBeNull();
        
        // Act - Create response DTO using constructor
        var constructor = responseType!.GetConstructor([typeof(TestUser)]);
        constructor.ShouldNotBeNull();
        
        var responseDto = constructor!.Invoke([user]);

        // Assert
        var idProp = responseType.GetProperty("Id")!;
        var firstNameProp = responseType.GetProperty("FirstName")!;
        var lastNameProp = responseType.GetProperty("LastName")!;
        var emailProp = responseType.GetProperty("Email")!;
        var isActiveProp = responseType.GetProperty("IsActive")!;

        idProp.GetValue(responseDto).ShouldBe(user.Id);
        firstNameProp.GetValue(responseDto).ShouldBe(user.FirstName);
        lastNameProp.GetValue(responseDto).ShouldBe(user.LastName);
        emailProp.GetValue(responseDto).ShouldBe(user.Email);
        isActiveProp.GetValue(responseDto).ShouldBe(user.IsActive);
    }

    [Fact]
    public void QueryDto_ShouldWork_ForSearchScenario()
    {
        // Arrange
        var assembly = Assembly.GetAssembly(typeof(TestUser));
        var queryType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserQuery");
        
        queryType.ShouldNotBeNull();
        var queryDto = Activator.CreateInstance(queryType!);

        // Act - Simulate search/filter scenario
        var firstNameProp = queryType.GetProperty("FirstName")!;
        var isActiveProp = queryType.GetProperty("IsActive")!;

        firstNameProp.SetValue(queryDto, "John");
        isActiveProp.SetValue(queryDto, true);

        // Assert
        firstNameProp.GetValue(queryDto).ShouldBe("John");
        isActiveProp.GetValue(queryDto).ShouldBe(true);

        // Test nullable behavior - should accept null values
        firstNameProp.SetValue(queryDto, null);
        firstNameProp.GetValue(queryDto).ShouldBeNull("Query DTO properties should accept null for optional filtering");
    }

    [Fact]
    public void UpsertDto_ShouldWork_ForCreateOrUpdateScenario()
    {
        // Arrange
        var assembly = Assembly.GetAssembly(typeof(TestUser));
        var upsertType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.UpsertTestUserRequest");
        
        upsertType.ShouldNotBeNull();

        // Test Create scenario (no Id)
        var createUpsertDto = Activator.CreateInstance(upsertType!);
        var idProp = upsertType.GetProperty("Id")!;
        var firstNameProp = upsertType.GetProperty("FirstName")!;

        // Act - Create scenario
        firstNameProp.SetValue(createUpsertDto, "New User");
        // Leave Id as default (0 or null depending on type)

        // Assert - Create scenario
        firstNameProp.GetValue(createUpsertDto).ShouldBe("New User");
        var idValue = idProp.GetValue(createUpsertDto);
        // Id should be default value (0 for int)
        idValue.ShouldBe(0);

        // Test Update scenario (with Id)
        var updateUpsertDto = Activator.CreateInstance(upsertType);

        // Act - Update scenario
        idProp.SetValue(updateUpsertDto, 123);
        firstNameProp.SetValue(updateUpsertDto, "Updated User");

        // Assert - Update scenario
        idProp.GetValue(updateUpsertDto).ShouldBe(123);
        firstNameProp.GetValue(updateUpsertDto).ShouldBe("Updated User");
    }

    [Fact]
    public void ProjectionProperty_ShouldWork_ForLinqQueries()
    {
        // Arrange
        var assembly = Assembly.GetAssembly(typeof(TestUser));
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserResponse");
        
        responseType.ShouldNotBeNull();
        
        var users = new List<TestUser>
        {
            CreateSampleTestUser(1, "Alice", "Smith", "alice@example.com"),
            CreateSampleTestUser(2, "Bob", "Johnson", "bob@example.com"),
            CreateSampleTestUser(3, "Charlie", "Brown", "charlie@example.com")
        };

        // Act
        var projectionProperty = responseType!.GetProperty("Projection", BindingFlags.Public | BindingFlags.Static);
        projectionProperty.ShouldNotBeNull();
        
        var projection = projectionProperty!.GetValue(null);
        projection.ShouldNotBeNull();

        // Use projection in a LINQ query (simulate)
        var queryable = users.AsQueryable();
        
        // This simulates: users.Select(TestUserResponse.Projection)
        // Cast to proper Expression type and compile it
        if (projection is System.Linq.Expressions.LambdaExpression lambdaExpr)
        {
            var compiledDelegate = lambdaExpr.Compile();
            var projectedResults = users.Select(user => compiledDelegate.DynamicInvoke(user)).ToList();

            // Assert
            projectedResults.Count().ShouldBe(3);
            
            var firstResult = projectedResults[0];
            var firstResultType = firstResult?.GetType();
            firstResultType.ShouldBe(responseType);
            
            var firstNameProp = responseType.GetProperty("FirstName")!;
            firstNameProp.GetValue(firstResult).ShouldBe("Alice");
        }
        else
        {
            Assert.Fail("Projection property should be of type LambdaExpression");
        }
    }

    [Fact]
    public void AuditableDtos_ShouldExcludeAuditFields_InPracticalScenarios()
    {
        // Arrange
        var product = CreateSampleTestProduct();
        var assembly = Assembly.GetAssembly(typeof(TestProduct));
        var createType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.CreateTestProductRequest");
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestProductResponse");
        
        createType.ShouldNotBeNull();
        responseType.ShouldNotBeNull();

        // Act - Test Create DTO doesn't have audit fields
        var createDto = Activator.CreateInstance(createType!);
        var nameProp = createType.GetProperty("Name")!;
        var priceProp = createType.GetProperty("Price")!;
        
        nameProp.SetValue(createDto, "New Product");
        priceProp.SetValue(createDto, 49.99m);

        // Assert - Create DTO
        nameProp.GetValue(createDto).ShouldBe("New Product");
        priceProp.GetValue(createDto).ShouldBe(49.99m);
        
        // Audit fields should not exist
        createType.GetProperty("CreatedAt").ShouldBeNull("Create DTO should not have audit field CreatedAt");
        createType.GetProperty("CreatedBy").ShouldBeNull("Create DTO should not have audit field CreatedBy");
        createType.GetProperty("UpdatedAt").ShouldBeNull("Create DTO should not have audit field UpdatedAt");
        createType.GetProperty("UpdatedBy").ShouldBeNull("Create DTO should not have audit field UpdatedBy");

        // Act - Test Response DTO (should also exclude audit fields for GenerateAuditableDtos)
        var responseDto = Activator.CreateInstance(responseType!, product);
        
        // Assert - Response DTO
        var responseNameProp = responseType!.GetProperty("Name")!;
        responseNameProp.GetValue(responseDto).ShouldBe(product.Name);
        
        // Audit fields should not exist in response either
        responseType.GetProperty("CreatedAt").ShouldBeNull("Response DTO should not have audit field CreatedAt");
        responseType.GetProperty("CreatedBy").ShouldBeNull("Response DTO should not have audit field CreatedBy");
    }

    #region Helper Methods

    private static TestUser CreateSampleTestUser(int id = 1, string firstName = "John", string lastName = "Doe", string email = "john.doe@example.com")
    {
        return new TestUser
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Password = "hashedpassword",
            DateOfBirth = new DateTime(1990, 1, 1),
            IsActive = true,
            LastLoginAt = DateTime.Now.AddHours(-2),
            CreatedAt = DateTime.Now.AddDays(-30)
        };
    }

    private static TestProduct CreateSampleTestProduct()
    {
        return new TestProduct
        {
            Id = 1,
            Name = "Sample Product",
            Description = "A sample product for testing",
            Price = 29.99m,
            IsAvailable = true,
            InternalNotes = "Internal notes here",
            CreatedAt = DateTime.Now.AddDays(-10),
            UpdatedAt = DateTime.Now.AddHours(-1),
            CreatedBy = "system",
            UpdatedBy = "admin"
        };
    }

    #endregion
}
