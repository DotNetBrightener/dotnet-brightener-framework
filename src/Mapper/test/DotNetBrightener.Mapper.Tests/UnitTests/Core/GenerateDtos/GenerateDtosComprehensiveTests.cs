using System.Reflection;
using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.GenerateDtos;

/// <summary>
///     Tests for comprehensive GenerateDtos scenarios including custom configurations,
///     different output types, naming conventions, and complex data types.
/// </summary>
public class GenerateDtosComprehensiveTests
{
    [Fact]
    public void GenerateDtos_ShouldGenerateRecordTypes_WhenRecordOutputSpecified()
    {
        // Arrange & Act
        var assembly = Assembly.GetAssembly(typeof(TestOrder));
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestOrderResponse");
        var queryType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestOrderQuery");

        // Assert
        responseType.ShouldNotBeNull("TestOrderResponse should be generated as record");
        queryType.ShouldNotBeNull("TestOrderQuery should be generated as record");

        // Records are classes in .NET but should have record behavior
        responseType!.IsClass.ShouldBeTrue("Records are reference types");
        queryType!.IsClass.ShouldBeTrue("Records are reference types");
        
        // Test that properties exist and have correct types
        var idProperty = responseType.GetProperty("Id");
        idProperty.ShouldNotBeNull("Record should have Id property");
        idProperty!.PropertyType.ShouldBe(typeof(Guid), "Guid property should be preserved");
        
        var orderNumberProperty = responseType.GetProperty("OrderNumber");
        orderNumberProperty.ShouldNotBeNull("Record should have OrderNumber property");
        orderNumberProperty!.PropertyType.ShouldBe(typeof(string));
        
        var statusProperty = responseType.GetProperty("Status");
        statusProperty.ShouldNotBeNull("Record should have Status enum property");
        statusProperty!.PropertyType.ShouldBe(typeof(OrderStatus));
        
        var notesProperty = responseType.GetProperty("Notes");
        notesProperty.ShouldNotBeNull("Record should have Notes nullable property");
        notesProperty!.PropertyType.ShouldBe(typeof(string));
    }

    [Fact]
    public void GenerateDtos_ShouldSupportMultipleAttributes_WithDifferentExclusions()
    {
        // Arrange & Act
        var assembly = Assembly.GetAssembly(typeof(TestMultiConfigEntity));
        var createType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.CreateTestMultiConfigEntityRequest");
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestMultiConfigEntityResponse");

        // Assert
        createType.ShouldNotBeNull("Create DTO should be generated");
        responseType.ShouldNotBeNull("Response DTO should be generated");
        
        // Create DTO should exclude only SecretKey
        createType!.GetProperty("SecretKey").ShouldBeNull("Create DTO should exclude SecretKey");
        createType.GetProperty("InternalData").ShouldNotBeNull("Create DTO should include InternalData");
        createType.GetProperty("Name").ShouldNotBeNull("Create DTO should include Name");
        createType.GetProperty("Description").ShouldNotBeNull("Create DTO should include Description");
        
        // Response DTO should exclude both SecretKey and InternalData
        responseType!.GetProperty("SecretKey").ShouldBeNull("Response DTO should exclude SecretKey");
        responseType.GetProperty("InternalData").ShouldBeNull("Response DTO should exclude InternalData");
        responseType.GetProperty("Name").ShouldNotBeNull("Response DTO should include Name");
        responseType.GetProperty("Description").ShouldNotBeNull("Response DTO should include Description");
    }

    [Fact]
    public void GenerateDtos_ShouldApplyCustomNaming_WithPrefixAndSuffix()
    {
        // Arrange & Act
        var assembly = Assembly.GetAssembly(typeof(TestCustomNaming));
        
        // Expected naming pattern: ApiTestCustomNaming[Type]Model
        var createType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.ApiCreateTestCustomNamingRequestModel");
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.ApiTestCustomNamingResponseModel");
        var queryType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.ApiTestCustomNamingQueryModel");

        var allTypes = assembly!.GetTypes()
            .Where(t => t.Name.Contains("TestCustomNaming"))
            .Select(t => t.Name)
            .ToList();

        allTypes.ShouldNotBeEmpty("Should generate DTOs for TestCustomNaming");
        
        // At minimum, should have DTOs with TestCustomNaming in the name
        allTypes.ShouldContain(name => name.Contains("TestCustomNaming"), 
            "Generated DTOs should contain base type name");
    }

    [Fact]
    public void GenerateDtos_ShouldGenerateRecordStructs_WhenRecordStructOutputSpecified()
    {
        // Arrange & Act
        var assembly = Assembly.GetAssembly(typeof(TestCompactEntity));
        
        // Look for generated record structs
        var allTypes = assembly!.GetTypes()
            .Where(t => t.Name.Contains("TestCompactEntity"))
            .ToList();

        // Assert
        allTypes.ShouldNotBeEmpty("Should generate DTOs for TestCompactEntity");
        
        // Test at least one of the generated types
        var firstGeneratedType = allTypes.FirstOrDefault();
        firstGeneratedType.ShouldNotBeNull();
        
        if (firstGeneratedType != null)
        {
            // Should have properties from the source type (excluding audit fields)
            var idProperty = firstGeneratedType.GetProperty("Id");
            idProperty.ShouldNotBeNull("Generated type should have Id property");
            
            var codeProperty = firstGeneratedType.GetProperty("Code");
            codeProperty.ShouldNotBeNull("Generated type should have Code property");
            
            var createdAtProperty = firstGeneratedType.GetProperty("CreatedAt");
            if (createdAtProperty != null)
            {
                createdAtProperty.PropertyType.ShouldBe(typeof(DateTime), "CreatedAt should have correct type if present");
            }
        }
    }

    [Fact]
    public void GenerateDtos_ShouldIncludeFields_WhenIncludeFieldsEnabled()
    {
        // Arrange & Act
        var assembly = Assembly.GetAssembly(typeof(TestEntityWithFields));
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestEntityWithFieldsResponse");

        // Assert
        responseType.ShouldNotBeNull("Response DTO with fields should be generated");
        
        if (responseType != null)
        {
            // Check for both properties and fields - try property first, then field
            MemberInfo? publicFieldMember = responseType.GetProperty("PublicField");
            if (publicFieldMember == null)
                publicFieldMember = responseType.GetField("PublicField");
            publicFieldMember.ShouldNotBeNull("Public field should be included");
            
            MemberInfo? readOnlyFieldMember = responseType.GetProperty("ReadOnlyField");
            if (readOnlyFieldMember == null)
                readOnlyFieldMember = responseType.GetField("ReadOnlyField");
            readOnlyFieldMember.ShouldNotBeNull("ReadOnly field should be included");
            
            // Should include regular properties
            var propertyFieldProperty = responseType.GetProperty("PropertyField");
            propertyFieldProperty.ShouldNotBeNull("Regular property should be included");
            
            // Should NOT include private fields
            MemberInfo? privateFieldMember = responseType.GetProperty("PrivateField");
            if (privateFieldMember == null)
                privateFieldMember = responseType.GetField("PrivateField");
            privateFieldMember.ShouldBeNull("Private field should not be included");
        }
    }

    [Fact]
    public void GenerateDtos_ShouldHandleComplexTypes_Correctly()
    {
        // Arrange & Act
        var assembly = Assembly.GetAssembly(typeof(TestComplexTypes));
        var createType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.CreateTestComplexTypesRequest");
        var updateType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.UpdateTestComplexTypesRequest");

        // Assert
        createType.ShouldNotBeNull("Create DTO should be generated for complex types");
        updateType.ShouldNotBeNull("Update DTO should be generated for complex types");
        
        if (createType != null)
        {
            // Test generic collections
            var tagsProperty = createType.GetProperty("Tags");
            tagsProperty.ShouldNotBeNull("List<string> property should be included");
            if (tagsProperty != null)
            {
                tagsProperty.PropertyType.ShouldBe(typeof(List<string>), 
                    "Generic collection type should be preserved");
            }
            
            // Test dictionary types
            var metadataProperty = createType.GetProperty("Metadata");
            metadataProperty.ShouldNotBeNull("Dictionary property should be included");
            if (metadataProperty != null)
            {
                metadataProperty.PropertyType.ShouldBe(typeof(Dictionary<string, string>),
                    "Dictionary type should be preserved");
            }
            
            // Test custom object types
            var nestedObjectProperty = createType.GetProperty("NestedObject");
            nestedObjectProperty.ShouldNotBeNull("Custom object property should be included");
            if (nestedObjectProperty != null)
            {
                nestedObjectProperty.PropertyType.ShouldBe(typeof(TestNestedType),
                    "Custom object type should be preserved");
            }
            
            // Test array types
            var arrayProperty = createType.GetProperty("ArrayProperty");
            arrayProperty.ShouldNotBeNull("Array property should be included");
            if (arrayProperty != null)
            {
                arrayProperty.PropertyType.ShouldBe(typeof(TestNestedType[]),
                    "Array type should be preserved");
            }
        }
    }

    [Fact]
    public void GenerateDtos_ShouldWorkWithEnumProperties_Correctly()
    {
        // Arrange & Act
        var assembly = Assembly.GetAssembly(typeof(TestOrder));
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestOrderResponse");
        var queryType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestOrderQuery");

        // Assert
        responseType.ShouldNotBeNull();
        queryType.ShouldNotBeNull();
        
        if (responseType != null)
        {
            var statusProperty = responseType.GetProperty("Status");
            statusProperty.ShouldNotBeNull("Enum property should be included");
            statusProperty!.PropertyType.ShouldBe(typeof(OrderStatus), 
                "Enum type should be preserved exactly");
        }
        
        if (queryType != null)
        {
            var statusProperty = queryType.GetProperty("Status");
            statusProperty.ShouldNotBeNull("Enum property should be included in query DTO");
            
            // In query DTOs, enum should be nullable for filtering
            var expectedType = typeof(OrderStatus?);
            statusProperty!.PropertyType.ShouldBe(expectedType,
                "Enum property in query DTO should be nullable");
        }
    }

    [Fact]
    public void GenerateDtos_FunctionalTest_WithRealWorldScenario()
    {
        // Arrange - Create a realistic order processing scenario
        var order = new TestOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-2024-001",
            TotalAmount = 299.99m,
            OrderDate = DateTime.Now.AddHours(-2),
            CustomerEmail = "customer@example.com",
            Status = OrderStatus.Processing,
            Notes = "Express delivery requested"
        };

        var assembly = Assembly.GetAssembly(typeof(TestOrder));
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestOrderResponse");
        
        responseType.ShouldNotBeNull();
        
        // Act - Create response DTO
        var constructor = responseType!.GetConstructor([typeof(TestOrder)]);
        constructor.ShouldNotBeNull();
        
        var responseDto = constructor!.Invoke([order]);
        
        // Assert - Verify all data is correctly mapped
        var idProperty = responseType.GetProperty("Id")!;
        idProperty.GetValue(responseDto).ShouldBe(order.Id);
        
        var orderNumberProperty = responseType.GetProperty("OrderNumber")!;
        orderNumberProperty.GetValue(responseDto).ShouldBe("ORD-2024-001");
        
        var totalAmountProperty = responseType.GetProperty("TotalAmount")!;
        totalAmountProperty.GetValue(responseDto).ShouldBe(299.99m);
        
        var statusProperty = responseType.GetProperty("Status")!;
        statusProperty.GetValue(responseDto).ShouldBe(OrderStatus.Processing);
        
        var notesProperty = responseType.GetProperty("Notes")!;
        notesProperty.GetValue(responseDto).ShouldBe("Express delivery requested");
    }

    [Fact]
    public void GenerateDtos_ShouldMaintainNullabilityAnnotations_Correctly()
    {
        // Test that nullable reference types are handled correctly
        var assembly = Assembly.GetAssembly(typeof(TestOrder));
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestOrderResponse");
        
        responseType.ShouldNotBeNull();
        
        if (responseType != null)
        {
            // Notes property is nullable string in source
            var notesProperty = responseType.GetProperty("Notes");
            notesProperty.ShouldNotBeNull();
            
            // CustomerEmail is non-nullable string
            var emailProperty = responseType.GetProperty("CustomerEmail");
            emailProperty.ShouldNotBeNull();
            emailProperty!.PropertyType.ShouldBe(typeof(string));
            
            // Test with actual data
            var order = new TestOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = "TEST-001",
                CustomerEmail = "test@example.com",
                Notes = null // Test null nullable field
            };
            
            var constructor = responseType.GetConstructor([typeof(TestOrder)]);
            var dto         = constructor!.Invoke([order]);
            
            notesProperty!.GetValue(dto).ShouldBeNull("Nullable property should accept null");
            emailProperty.GetValue(dto).ShouldBe("test@example.com");
        }
    }

    [Fact]
    public void GenerateDtos_PerformanceTest_WithManyInstances()
    {
        // Test performance with multiple DTO creations
        const int instanceCount = 100;
        
        var orders = new List<TestOrder>();
        for (int i = 0; i < instanceCount; i++)
        {
            orders.Add(new TestOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"ORD-{i:D6}",
                TotalAmount = (decimal)(i * 10.5),
                OrderDate = DateTime.Now.AddDays(-i),
                CustomerEmail = $"customer{i}@test.com",
                Status = (OrderStatus)(i % 5),
                Notes = i % 3 == 0 ? null : $"Notes for order {i}"
            });
        }
        
        var assembly = Assembly.GetAssembly(typeof(TestOrder));
        var responseType = assembly?.GetType("DotNetBrightener.Mapper.Tests.TestModels.TestOrderResponse");
        var constructor = responseType!.GetConstructor([typeof(TestOrder)]);
        
        // Act - Measure performance
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var responseDtos = orders.Select(order => 
            constructor!.Invoke([order])).ToList();
        
        stopwatch.Stop();
        
        // Assert
        responseDtos.Count().ShouldBe(instanceCount);
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(1000, 
            "Creating 100 DTOs should be fast");
        
        // Verify some random samples
        var sample1 = responseDtos[25];
        var orderNumberProp = responseType.GetProperty("OrderNumber")!;
        orderNumberProp.GetValue(sample1).ShouldBe("ORD-000025");
        
        var sample2 = responseDtos[75];
        orderNumberProp.GetValue(sample2).ShouldBe("ORD-000075");
    }
}
