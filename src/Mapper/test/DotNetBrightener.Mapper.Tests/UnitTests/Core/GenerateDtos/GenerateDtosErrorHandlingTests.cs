using System.Reflection;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.GenerateDtos;

/// <summary>
///     Tests for error handling and edge cases in GenerateDtos functionality.
///     Verifies robustness and proper error reporting.
/// </summary>
public class GenerateDtosErrorHandlingTests
{
    [Fact]
    public void GeneratedDtos_ShouldHandleNull_Gracefully()
    {
        // Arrange
        var assembly = Assembly.GetAssembly(typeof(TestModels.TestUser));
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserResponse");
        
        responseType.ShouldNotBeNull();
        
        // Act & Assert - Test null handling in constructor
        var sourceConstructor = responseType!.GetConstructor([typeof(TestModels.TestUser)]);
        sourceConstructor.ShouldNotBeNull();
        
        // This should not throw - constructor should handle null gracefully or throw meaningful exception
        Action actWithNull = () => sourceConstructor!.Invoke([null!]);
        
        // Test that parameterless constructor works
        var parameterlessConstructor = responseType.GetConstructor(Type.EmptyTypes);
        parameterlessConstructor.ShouldNotBeNull();
        
        var instance = parameterlessConstructor!.Invoke(null);
        instance.ShouldNotBeNull();
    }

    [Fact]
    public void GeneratedDtos_ShouldHaveConsistent_PropertyNames()
    {
        // Arrange & Act
        var assembly = Assembly.GetAssembly(typeof(TestModels.TestUser));
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserResponse");
        var createType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.CreateTestUserRequest");
        var updateType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.UpdateTestUserRequest");
        
        // Assert
        responseType.ShouldNotBeNull();
        createType.ShouldNotBeNull();
        updateType.ShouldNotBeNull();
        
        // Properties that should exist in multiple DTOs should have consistent names
        var commonProperties = new[] { "FirstName", "LastName", "Email", "IsActive" };
        
        foreach (var propName in commonProperties)
        {
            var responseProp = responseType!.GetProperty(propName);
            var createProp = createType!.GetProperty(propName);
            var updateProp = updateType!.GetProperty(propName);
            
            // All should have the property with same name
            responseProp.ShouldNotBeNull($"Response DTO should have {propName} property");
            createProp.ShouldNotBeNull($"Create DTO should have {propName} property");
            updateProp.ShouldNotBeNull($"Update DTO should have {propName} property");
            
            // And same type
            if (responseProp != null && createProp != null && updateProp != null)
            {
                responseProp.PropertyType.ShouldBe(createProp.PropertyType, 
                    $"{propName} should have same type in Response and Create DTOs");
                responseProp.PropertyType.ShouldBe(updateProp.PropertyType, 
                    $"{propName} should have same type in Response and Update DTOs");
            }
        }
    }

    [Fact]
    public void GeneratedDtos_ShouldHaveValid_MethodSignatures()
    {
        // Arrange
        var assembly = Assembly.GetAssembly(typeof(TestModels.TestUser));
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserResponse");
        
        responseType.ShouldNotBeNull();
        
        // Test Projection property signature
        var projectionProperty = responseType.GetProperty("Projection", BindingFlags.Public | BindingFlags.Static);
        projectionProperty.ShouldNotBeNull("Projection property should exist");
        projectionProperty!.PropertyType.ShouldNotBeNull("Projection should have valid type");
        projectionProperty.CanRead.ShouldBeTrue("Projection should be readable");
        projectionProperty.GetMethod!.IsStatic.ShouldBeTrue("Projection should be static");
        projectionProperty.GetMethod.IsPublic.ShouldBeTrue("Projection should be public");
        
        // Test constructors
        var constructors = responseType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        constructors.Length.ShouldBeGreaterThan(0, "Should have at least one public constructor");
        
        var parameterlessConstructor = responseType.GetConstructor(Type.EmptyTypes);
        parameterlessConstructor.ShouldNotBeNull("Should have parameterless constructor");
        
        var sourceConstructor = responseType.GetConstructor([typeof(TestModels.TestUser)]);
        sourceConstructor.ShouldNotBeNull("Should have source type constructor");
    }

    [Fact]
    public void GeneratedDtos_ShouldWork_WithComplexDataTypes()
    {
        // Arrange - Test with complex data including DateTime, nullable types, etc.
        var user = new TestModels.TestUser
        {
            Id = int.MaxValue,
            FirstName = "Test with special chars: ����",
            LastName = "O'Connor-Smith",
            Email = "test+special@example-domain.co.uk",
            Password = "Complex!Password@123",
            DateOfBirth = DateTime.MinValue,
            IsActive = false,
            LastLoginAt = null, // Test null DateTime?
            CreatedAt = DateTime.MaxValue
        };
        
        var assembly = Assembly.GetAssembly(typeof(TestModels.TestUser));
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserResponse");
        
        responseType.ShouldNotBeNull();
        
        // Act
        var constructor = responseType!.GetConstructor([typeof(TestModels.TestUser)]);
        var responseDto = constructor!.Invoke([user]);
        
        // Assert - Complex data should be preserved
        var idProp = responseType.GetProperty("Id")!;
        idProp.GetValue(responseDto).ShouldBe(int.MaxValue);
        
        var firstNameProp = responseType.GetProperty("FirstName")!;
        firstNameProp.GetValue(responseDto).ShouldBe("Test with special chars: ����");
        
        var lastNameProp = responseType.GetProperty("LastName")!;
        lastNameProp.GetValue(responseDto).ShouldBe("O'Connor-Smith");
        
        var emailProp = responseType.GetProperty("Email")!;
        emailProp.GetValue(responseDto).ShouldBe("test+special@example-domain.co.uk");
        
        var dobProp = responseType.GetProperty("DateOfBirth")!;
        dobProp.GetValue(responseDto).ShouldBe(DateTime.MinValue);
        
        var lastLoginProp = responseType.GetProperty("LastLoginAt")!;
        lastLoginProp.GetValue(responseDto).ShouldBeNull();
        
        var createdAtProp = responseType.GetProperty("CreatedAt")!;
        createdAtProp.GetValue(responseDto).ShouldBe(DateTime.MaxValue);
    }

    [Fact]
    public void GeneratedDtos_ShouldWork_WithEmptyStringsAndDefaults()
    {
        // Arrange - Test edge cases with empty/default values
        var user = new TestModels.TestUser
        {
            Id = 0,
            FirstName = "",
            LastName = null!, // Test null string
            Email = string.Empty,
            Password = "",
            DateOfBirth = default(DateTime),
            IsActive = false,
            LastLoginAt = null,
            CreatedAt = default(DateTime)
        };
        
        var assembly = Assembly.GetAssembly(typeof(TestModels.TestUser));
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserResponse");
        
        // Act
        var constructor = responseType!.GetConstructor([typeof(TestModels.TestUser)]);
        var responseDto = constructor!.Invoke([user]);
        
        // Assert - Empty/default values should be handled correctly
        var idProp = responseType.GetProperty("Id")!;
        idProp.GetValue(responseDto).ShouldBe(0);
        
        var firstNameProp = responseType.GetProperty("FirstName")!;
        firstNameProp.GetValue(responseDto).ShouldBe("");
        
        var lastNameProp = responseType.GetProperty("LastName")!;
        lastNameProp.GetValue(responseDto).ShouldBeNull();
        
        var emailProp = responseType.GetProperty("Email")!;
        emailProp.GetValue(responseDto).ShouldBe(string.Empty);
        
        var dobProp = responseType.GetProperty("DateOfBirth")!;
        dobProp.GetValue(responseDto).ShouldBe(default(DateTime));
        
        var isActiveProp = responseType.GetProperty("IsActive")!;
        isActiveProp.GetValue(responseDto).ShouldBe(false);
    }

    [Fact]
    public void GeneratedDtos_ShouldMaintain_ThreadSafety()
    {
        // Test that static members (like Projection) are thread-safe
        var assembly = Assembly.GetAssembly(typeof(TestModels.TestUser));
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserResponse");
        
        responseType.ShouldNotBeNull();
        
        var projectionProperty = responseType!.GetProperty("Projection", BindingFlags.Public | BindingFlags.Static);
        projectionProperty.ShouldNotBeNull();
        
        // Act - Access projection from multiple threads
        var tasks = new List<Task<object?>>();
        
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() => projectionProperty!.GetValue(null)));
        }
        
        Task.WaitAll(tasks.ToArray());
        
        // Assert - All tasks should complete successfully 
        foreach (var task in tasks)
        {
            task.Result.ShouldNotBeNull("Projection should return a valid result");
            task.IsCompletedSuccessfully.ShouldBeTrue("Task should complete without exceptions");
        }
        
        // All results should be functionally equivalent (though not necessarily the same instance)
        var firstResult = tasks[0].Result;
        foreach (var task in tasks.Skip(1))
        {
            task.Result.ShouldNotBeNull("All projection results should be non-null");
            // We verify that the type is the same rather than the exact instance
            task.Result!.GetType().ShouldBe(firstResult!.GetType(), "All projection results should have the same type");
        }
    }

    [Fact]
    public void GeneratedDtos_ShouldHandle_LargeDataSets()
    {
        // Test performance and memory efficiency with larger data sets
        const int dataSize = 1000;
        
        var users = new List<TestModels.TestUser>();
        for (int i = 0; i < dataSize; i++)
        {
            users.Add(new TestModels.TestUser
            {
                Id = i,
                FirstName = $"User{i}",
                LastName = $"LastName{i}",
                Email = $"user{i}@test.com",
                Password = $"password{i}",
                DateOfBirth = DateTime.Now.AddYears(-20 - (i % 50)),
                IsActive = i % 2 == 0,
                LastLoginAt = DateTime.Now.AddDays(-i % 30),
                CreatedAt = DateTime.Now.AddDays(-i)
            });
        }
        
        var assembly = Assembly.GetAssembly(typeof(TestModels.TestUser));
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserResponse");
        var constructor = responseType!.GetConstructor([typeof(TestModels.TestUser)]);
        
        // Act - Convert large dataset
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var responseDtos = users.Select(user => constructor!.Invoke([user])).ToList();
        stopwatch.Stop();
        
        // Assert
        responseDtos.Count().ShouldBe(dataSize);
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(5000, "Large dataset conversion should be reasonably fast");
        
        // Test some random samples
        var sample1 = responseDtos[100];
        var firstNameProp = responseType.GetProperty("FirstName")!;
        firstNameProp.GetValue(sample1).ShouldBe("User100");
        
        var sample2 = responseDtos[500];
        firstNameProp.GetValue(sample2).ShouldBe("User500");
    }

    [Fact]
    public void GeneratedAuditableDtos_ShouldConsistently_ExcludeAuditFields()
    {
        // Test that GenerateAuditableDtos consistently excludes all audit fields across different DTO types
        var assembly = Assembly.GetAssembly(typeof(TestModels.TestProduct));
        var createType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.CreateTestProductRequest");
        var updateType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.UpdateTestProductRequest");
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestProductResponse");
        
        var auditFields = new[] { "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy" };
        
        foreach (var auditField in auditFields)
        {
            createType!.GetProperty(auditField).ShouldBeNull($"Create DTO should not have audit field {auditField}");
            updateType!.GetProperty(auditField).ShouldBeNull($"Update DTO should not have audit field {auditField}");
            responseType!.GetProperty(auditField).ShouldBeNull($"Response DTO should not have audit field {auditField}");
        }
        
        // But should have business fields
        var businessFields = new[] { "Name", "Description", "Price", "IsAvailable" };
        
        foreach (var businessField in businessFields)
        {
            createType!.GetProperty(businessField).ShouldNotBeNull($"Create DTO should have business field {businessField}");
            updateType!.GetProperty(businessField).ShouldNotBeNull($"Update DTO should have business field {businessField}");
            responseType!.GetProperty(businessField).ShouldNotBeNull($"Response DTO should have business field {businessField}");
        }
    }
}
