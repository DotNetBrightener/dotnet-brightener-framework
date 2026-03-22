using System.Reflection;
using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.GenerateDtos;

/// <summary>
///     Tests for individual DTO type creation with GenerateDtos attribute.
///     Verifies each DTO type (Create, Update, Response, Query, Upsert) is generated correctly.
/// </summary>
public class GenerateDtosCreationTests
{
    [Fact]
    public void GenerateDtos_ShouldGenerateCreateDto_WhenCreateTypeSpecified()
    {
        // Arrange & Act
        var assembly = Assembly.GetAssembly(typeof(TestUser));
        var createType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.CreateTestUserRequest");

        // Assert
        createType.ShouldNotBeNull("CreateTestUserRequest should be generated");
        
        // Create DTO should exclude Id by default
        var idProperty = createType!.GetProperty("Id");
        idProperty.ShouldBeNull("Create DTOs should exclude Id property by default");
        
        // Should include other properties
        var firstNameProperty = createType.GetProperty("FirstName");
        firstNameProperty.ShouldNotBeNull("Create DTO should include FirstName");
        
        var passwordProperty = createType.GetProperty("Password");
        passwordProperty.ShouldNotBeNull("Create DTO should include Password for creation");
    }

    [Fact]
    public void GenerateDtos_ShouldGenerateUpdateDto_WhenUpdateTypeSpecified()
    {
        // Arrange & Act
        var assembly = Assembly.GetAssembly(typeof(TestUser));
        var updateType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.UpdateTestUserRequest");

        // Assert
        updateType.ShouldNotBeNull("UpdateTestUserRequest should be generated");
        
        // Update DTO should include Id for identification
        var idProperty = updateType!.GetProperty("Id");
        idProperty.ShouldNotBeNull("Update DTOs should include Id property for identification");
        
        var firstNameProperty = updateType.GetProperty("FirstName");
        firstNameProperty.ShouldNotBeNull("Update DTO should include FirstName");
        
        var passwordProperty = updateType.GetProperty("Password");
        passwordProperty.ShouldNotBeNull("Update DTO should include Password for updates");
    }

    [Fact]
    public void GenerateDtos_ShouldGenerateResponseDto_WhenResponseTypeSpecified()
    {
        // Arrange & Act
        var assembly = Assembly.GetAssembly(typeof(TestUser));
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserResponse");

        // Assert
        responseType.ShouldNotBeNull("TestUserResponse should be generated");
        
        // Response DTO should include all non-excluded properties
        var idProperty = responseType!.GetProperty("Id");
        idProperty.ShouldNotBeNull("Response DTO should include Id");
        
        var firstNameProperty = responseType.GetProperty("FirstName");
        firstNameProperty.ShouldNotBeNull("Response DTO should include FirstName");
        
        var passwordProperty = responseType.GetProperty("Password");
        passwordProperty.ShouldNotBeNull("Response DTO should include Password (no exclusions specified)");
    }

    [Fact]
    public void GenerateDtos_ShouldGenerateQueryDto_WhenQueryTypeSpecified()
    {
        // Arrange & Act
        var assembly = Assembly.GetAssembly(typeof(TestUser));
        var queryType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserQuery");

        // Assert
        queryType.ShouldNotBeNull("TestUserQuery should be generated");
        
        // Query DTO properties should be nullable for filtering
        var firstNameProperty = queryType!.GetProperty("FirstName");
        firstNameProperty.ShouldNotBeNull("Query DTO should include FirstName");
        
        // Check if property type is nullable (string is reference type, so should be nullable)
        var isActiveProperty = queryType.GetProperty("IsActive");
        isActiveProperty.ShouldNotBeNull("Query DTO should include IsActive");
        
        // For value types, should be nullable
        if (isActiveProperty!.PropertyType.IsValueType)
        {
            var underlyingType = Nullable.GetUnderlyingType(isActiveProperty.PropertyType);
            underlyingType.ShouldNotBeNull("Value type properties in Query DTOs should be nullable");
        }
    }

    [Fact]
    public void GenerateDtos_ShouldGenerateUpsertDto_WhenUpsertTypeSpecified()
    {
        // Arrange & Act
        var assembly = Assembly.GetAssembly(typeof(TestUser));
        var upsertType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.UpsertTestUserRequest");

        // Assert
        upsertType.ShouldNotBeNull("UpsertTestUserRequest should be generated");
        
        // Upsert DTO should include Id (can be null for create, populated for update)
        var idProperty = upsertType!.GetProperty("Id");
        idProperty.ShouldNotBeNull("Upsert DTOs should include Id property");
        
        var firstNameProperty = upsertType.GetProperty("FirstName");
        firstNameProperty.ShouldNotBeNull("Upsert DTO should include FirstName");
    }

    [Fact]
    public void GenerateDtos_ShouldGenerateAllDtoTypes_WhenAllSpecified()
    {
        // Arrange & Act
        var assembly = Assembly.GetAssembly(typeof(TestUser));
        
        var createType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.CreateTestUserRequest");
        var updateType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.UpdateTestUserRequest");
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserResponse");
        var queryType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserQuery");
        var upsertType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.UpsertTestUserRequest");

        // Assert
        createType.ShouldNotBeNull("CreateTestUserRequest should be generated when DtoTypes.All is used");
        updateType.ShouldNotBeNull("UpdateTestUserRequest should be generated when DtoTypes.All is used");
        responseType.ShouldNotBeNull("TestUserResponse should be generated when DtoTypes.All is used");
        queryType.ShouldNotBeNull("TestUserQuery should be generated when DtoTypes.All is used");
        upsertType.ShouldNotBeNull("UpsertTestUserRequest should be generated when DtoTypes.All is used");
    }

    [Fact]
    public void GenerateAuditableDtos_ShouldExcludeAuditFields_Automatically()
    {
        // Arrange & Act
        var assembly = Assembly.GetAssembly(typeof(TestProduct));
        var createType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.CreateTestProductRequest");
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestProductResponse");

        // Assert
        createType.ShouldNotBeNull("CreateTestProductRequest should be generated");
        responseType.ShouldNotBeNull("TestProductResponse should be generated");
        
        // Check that audit fields are excluded
        var createdAtProperty = createType!.GetProperty("CreatedAt");
        createdAtProperty.ShouldBeNull("Audit field CreatedAt should be excluded from Create DTO");
        
        var updatedAtProperty = createType.GetProperty("UpdatedAt");
        updatedAtProperty.ShouldBeNull("Audit field UpdatedAt should be excluded from Create DTO");
        
        var createdByProperty = createType.GetProperty("CreatedBy");
        createdByProperty.ShouldBeNull("Audit field CreatedBy should be excluded from Create DTO");
        
        var updatedByProperty = createType.GetProperty("UpdatedBy");
        updatedByProperty.ShouldBeNull("Audit field UpdatedBy should be excluded from Create DTO");
        
        // But business properties should be included
        var nameProperty = createType.GetProperty("Name");
        nameProperty.ShouldNotBeNull("Business property Name should be included");
        
        var priceProperty = createType.GetProperty("Price");
        priceProperty.ShouldNotBeNull("Business property Price should be included");
    }

    [Fact]
    public void GeneratedDtos_ShouldHaveCorrectOutputType_WhenClassSpecified()
    {
        // Arrange & Act
        var assembly = Assembly.GetAssembly(typeof(TestUser));
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserResponse");

        // Assert
        responseType.ShouldNotBeNull("TestUserResponse should be generated");
        responseType!.IsClass.ShouldBeTrue("Generated DTO should be a class when OutputType.Class is specified");
        responseType.IsValueType.ShouldBeFalse("Class types should not be value types");
    }

    [Fact]
    public void GeneratedDtos_ShouldHaveParameterlessConstructor_ByDefault()
    {
        // Arrange & Act
        var assembly = Assembly.GetAssembly(typeof(TestUser));
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserResponse");

        // Assert
        responseType.ShouldNotBeNull("TestUserResponse should be generated");
        
        var parameterlessConstructor = responseType!.GetConstructor(Type.EmptyTypes);
        parameterlessConstructor.ShouldNotBeNull("Generated DTOs should have parameterless constructor by default");
    }

    [Fact]
    public void GeneratedDtos_ShouldHaveSourceTypeConstructor_ByDefault()
    {
        // Arrange & Act
        var assembly = Assembly.GetAssembly(typeof(TestUser));
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserResponse");

        // Assert
        responseType.ShouldNotBeNull("TestUserResponse should be generated");
        
        var sourceConstructor = responseType!.GetConstructor([typeof(TestUser)]);
        sourceConstructor.ShouldNotBeNull("Generated DTOs should have source type constructor by default");
    }

    [Fact]
    public void GeneratedDtos_ShouldHaveProjectionProperty_ByDefault()
    {
        // Arrange & Act
        var assembly = Assembly.GetAssembly(typeof(TestUser));
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestUserResponse");

        // Assert
        responseType.ShouldNotBeNull("TestUserResponse should be generated");
        
        var projectionProperty = responseType!.GetProperty("Projection", BindingFlags.Public | BindingFlags.Static);
        projectionProperty.ShouldNotBeNull("Generated DTOs should have static Projection property by default");
        
        // Verify projection property type
        var expectedProjectionType = typeof(System.Linq.Expressions.Expression<>).MakeGenericType(
            typeof(Func<,>).MakeGenericType(typeof(TestUser), responseType));
        projectionProperty!.PropertyType.ShouldBe(expectedProjectionType, "Projection should be Expression<Func<TSource, TTarget>>");
    }
}
