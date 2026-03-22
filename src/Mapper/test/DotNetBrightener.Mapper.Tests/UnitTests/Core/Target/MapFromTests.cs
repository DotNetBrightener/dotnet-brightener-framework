namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

// Test entities
public class MapFromTestEntity
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class MapFromNestedEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public MapFromCompanyEntity? Company { get; set; }
}

public class MapFromCompanyEntity
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

// Simple property rename test
[MappingTarget<MapFromTestEntity>( GenerateToSource = true)]
public partial class MapFromSimpleTarget
{
    [MapFrom(nameof(MapFromTestEntity.FirstName), Reversible = true)]
    public string Name { get; set; } = string.Empty;
}

// Multiple property renames
[MappingTarget<MapFromTestEntity>( GenerateToSource = true)]
public partial class MapFromMultipleTarget
{
    [MapFrom(nameof(MapFromTestEntity.FirstName), Reversible = true)]
    public string GivenName { get; set; } = string.Empty;

    [MapFrom(nameof(MapFromTestEntity.LastName), Reversible = true)]
    public string FamilyName { get; set; } = string.Empty;
}

// Non-reversible mapping
[MappingTarget<MapFromTestEntity>( GenerateToSource = true)]
public partial class MapFromNonReversibleTarget
{
    [MapFrom(nameof(MapFromTestEntity.FirstName), Reversible = false)]
    public string Name { get; set; } = string.Empty;
}

// Exclude from projection
[MappingTarget<MapFromTestEntity>( GenerateToSource = true)]
public partial class MapFromNoProjectionTarget
{
    [MapFrom(nameof(MapFromTestEntity.FirstName), IncludeInProjection = false)]
    public string Name { get; set; } = string.Empty;
}

// Computed value - one-way mapping (default Reversible = false)
[MappingTarget<MapFromTestEntity>( GenerateToSource = true)]
public partial class MapFromComputedTarget
{
    // Computed from FirstName - cannot be reversed
    [MapFrom(nameof(MapFromTestEntity.FirstName))]
    public string DisplayName { get; set; } = string.Empty;

    // Computed from LastName - cannot be reversed
    [MapFrom(nameof(MapFromTestEntity.LastName))]
    public string Surname { get; set; } = string.Empty;
}

// Computed expression - FirstName + LastName = FullName
[MappingTarget<MapFromTestEntity>( GenerateToSource = true)]
public partial class MapFromExpressionTarget
{
    // Computed expression - cannot be reversed
    [MapFrom(nameof(MapFromTestEntity.FirstName) + " + \" \" + " + nameof(MapFromTestEntity.LastName))]
    public string FullName { get; set; } = string.Empty;
}

// Nested target with company
[MappingTarget<MapFromCompanyEntity>( GenerateToSource = true)]
public partial class MapFromCompanyTarget
{
    [MapFrom(nameof(MapFromCompanyEntity.CompanyName), Reversible = true)]
    public string Name { get; set; } = string.Empty;
}

[MappingTarget<MapFromNestedEntity>(
    NestedTargetTypes = [typeof(MapFromCompanyTarget)],
    GenerateToSource = true)]
public partial class MapFromNestedTarget;

// ========================================
// Nested Property Path Tests (not nested targets!)
// These test [MapFrom("Property.Nested.Path")] using string literals
// ========================================

public class MapFromNestedPropertyEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public MapFromCompanyEntity? Company { get; set; }
}

public class MapFromMultiLevelEntity
{
    public int Id { get; set; }
    public MapFromNestedPropertyEntity? Employee { get; set; }
}

// Test single-level nested property path: Company.CompanyName
[MappingTarget<MapFromNestedPropertyEntity>(
    exclude: [nameof(MapFromNestedPropertyEntity.Company)], // Exclude navigation property
    GenerateToSource = false)]
public partial class MapFromSingleLevelPathTarget
{
    // Use string literal to access nested property
    [MapFrom("Company.CompanyName")]
    public string CompanyName { get; set; } = string.Empty;

    [MapFrom("Company.Address")]
    public string CompanyAddress { get; set; } = string.Empty;
    
    // Same as above but not a hardcoded string literal - should generate same mapping
    [MapFrom(nameof(@MapFromNestedPropertyEntity.Company.CompanyName))]
    public string AlternativeCompanyName { get; set; } = string.Empty;
}

// Test multi-level nested property path: Employee.Company.CompanyName
[MappingTarget<MapFromMultiLevelEntity>(
    exclude: [nameof(MapFromMultiLevelEntity.Employee)], // Exclude navigation property
    GenerateToSource = false)]
public partial class MapFromMultiLevelPathTarget
{
    // Two levels deep
    [MapFrom("Employee.Company.CompanyName")]
    public string EmployeeCompanyName { get; set; } = string.Empty;

    // Two levels deep
    [MapFrom("Employee.Company.Address")]
    public string EmployeeCompanyAddress { get; set; } = string.Empty;

    // Single level
    [MapFrom("Employee.Name")]
    public string EmployeeName { get; set; } = string.Empty;
}

public class MapFromTests
{
    [Fact]
    public void Constructor_ShouldMapSimplePropertyRename()
    {
        // Arrange
        var entity = new MapFromTestEntity
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Age = 30
        };

        // Act
        var target = new MapFromSimpleTarget(entity);

        // Assert
        target.ShouldNotBeNull();
        target.Id.ShouldBe(1);
        target.Name.ShouldBe("John"); // Mapped from FirstName
        target.LastName.ShouldBe("Doe");
        target.Email.ShouldBe("john@example.com");
        target.Age.ShouldBe(30);
    }

    [Fact]
    public void Constructor_ShouldMapMultiplePropertyRenames()
    {
        // Arrange
        var entity = new MapFromTestEntity
        {
            Id = 2,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            Age = 25
        };

        // Act
        var target = new MapFromMultipleTarget(entity);

        // Assert
        target.ShouldNotBeNull();
        target.Id.ShouldBe(2);
        target.GivenName.ShouldBe("Jane"); // Mapped from FirstName
        target.FamilyName.ShouldBe("Smith"); // Mapped from LastName
        target.Email.ShouldBe("jane@example.com");
        target.Age.ShouldBe(25);
    }

    [Fact]
    public void ToSource_ShouldReverseSimpleMapping()
    {
        // Arrange
        var target = new MapFromSimpleTarget
        {
            Id = 3,
            Name = "Bob", // Should map back to FirstName
            LastName = "Wilson",
            Email = "bob@example.com",
            Age = 40
        };

        // Act
        var entity = target.ToSource();

        // Assert
        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(3);
        entity.FirstName.ShouldBe("Bob"); // Mapped from Name
        entity.LastName.ShouldBe("Wilson");
        entity.Email.ShouldBe("bob@example.com");
        entity.Age.ShouldBe(40);
    }

    [Fact]
    public void ToSource_ShouldReverseMultipleMappings()
    {
        // Arrange
        var target = new MapFromMultipleTarget
        {
            Id = 4,
            GivenName = "Alice", // Should map back to FirstName
            FamilyName = "Brown", // Should map back to LastName
            Email = "alice@example.com",
            Age = 35
        };

        // Act
        var entity = target.ToSource();

        // Assert
        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(4);
        entity.FirstName.ShouldBe("Alice");
        entity.LastName.ShouldBe("Brown");
        entity.Email.ShouldBe("alice@example.com");
        entity.Age.ShouldBe(35);
    }

    [Fact]
    public void ToSource_ShouldNotIncludeNonReversibleMapping()
    {
        // Arrange
        var target = new MapFromNonReversibleTarget
        {
            Id = 5,
            Name = "Charlie", // Should NOT map back (Reversible = false)
            LastName = "Davis",
            Email = "charlie@example.com",
            Age = 45
        };

        // Act
        var entity = target.ToSource();

        // Assert
        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(5);
        // FirstName should be default since Name is not reversible
        entity.FirstName.ShouldBeEmpty();
        entity.LastName.ShouldBe("Davis");
        entity.Email.ShouldBe("charlie@example.com");
        entity.Age.ShouldBe(45);
    }

    [Fact]
    public void Projection_ShouldMapSimplePropertyRename()
    {
        // Arrange
        var entities = new[]
        {
            new MapFromTestEntity { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Age = 30 },
            new MapFromTestEntity { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", Age = 25 }
        }.AsQueryable();

        // Act
        var targets = entities.Select(MapFromSimpleTarget.Projection).ToList();

        // Assert
        targets.Count().ShouldBe(2);
        targets[0].Id.ShouldBe(1);
        targets[0].Name.ShouldBe("John");
        targets[1].Id.ShouldBe(2);
        targets[1].Name.ShouldBe("Jane");
    }

    [Fact]
    public void Projection_ShouldMapMultiplePropertyRenames()
    {
        // Arrange
        var entities = new[]
        {
            new MapFromTestEntity { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Age = 30 }
        }.AsQueryable();

        // Act
        var targets = entities.Select(MapFromMultipleTarget.Projection).ToList();

        // Assert
        targets.Count().ShouldBe(1);
        targets[0].GivenName.ShouldBe("John");
        targets[0].FamilyName.ShouldBe("Doe");
    }

    [Fact]
    public void NestedTarget_ShouldMapPropertyRenameInNestedType()
    {
        // Arrange
        var entity = new MapFromNestedEntity
        {
            Id = 1,
            Name = "Employee",
            Company = new MapFromCompanyEntity
            {
                Id = 100,
                CompanyName = "Acme Corp",
                Address = "123 Main St"
            }
        };

        // Act
        var target = new MapFromNestedTarget(entity);

        // Assert
        target.ShouldNotBeNull();
        target.Id.ShouldBe(1);
        target.Name.ShouldBe("Employee");
        target.Company.ShouldNotBeNull();
        target.Company!.Id.ShouldBe(100);
        target.Company.Name.ShouldBe("Acme Corp"); // Mapped from CompanyName
        target.Company.Address.ShouldBe("123 Main St");
    }

    [Fact]
    public void NestedTarget_ToSource_ShouldReverseNestedMapping()
    {
        // Arrange
        var target = new MapFromNestedTarget
        {
            Id = 2,
            Name = "Manager",
            Company = new MapFromCompanyTarget
            {
                Id = 200,
                Name = "TechCo", // Should map back to CompanyName
                Address = "456 Tech Ave"
            }
        };

        // Act
        var entity = target.ToSource();

        // Assert
        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(2);
        entity.Name.ShouldBe("Manager");
        entity.Company.ShouldNotBeNull();
        entity.Company!.Id.ShouldBe(200);
        entity.Company.CompanyName.ShouldBe("TechCo");
        entity.Company.Address.ShouldBe("456 Tech Ave");
    }

    [Fact]
    public void SimplePropertyRename_ShouldNotGenerateDuplicateProperty()
    {
        // This test verifies that the target type has the correct properties
        // and doesn't have duplicate properties from both user-declared and generated

        // Arrange & Act
        var targetType = typeof(MapFromSimpleTarget);
        var properties = targetType.GetProperties();

        // Assert - should have Id, Name (not FirstName), LastName, Email, Age, and Projection
        var propertyNames = properties.Select(p => p.Name).ToList();
        propertyNames.ShouldContain("Id");
        propertyNames.ShouldContain("Name");
        propertyNames.ShouldContain("LastName");
        propertyNames.ShouldContain("Email");
        propertyNames.ShouldContain("Age");
        propertyNames.ShouldContain("Projection"); // Static property
        // Should NOT contain FirstName since it was mapped to Name
        propertyNames.ShouldNotContain("FirstName");
    }

    [Fact]
    public void MultiplePropertyRenames_ShouldNotGenerateDuplicateProperties()
    {
        // Arrange & Act
        var targetType = typeof(MapFromMultipleTarget);
        var properties = targetType.GetProperties();

        // Assert
        properties.Select(p => p.Name).ShouldContain("GivenName");
        properties.Select(p => p.Name).ShouldContain("FamilyName");
        properties.Select(p => p.Name).ShouldNotContain("FirstName");
        properties.Select(p => p.Name).ShouldNotContain("LastName");
    }

    [Fact]
    public void Constructor_ShouldMapComputedValues()
    {
        // Arrange
        var entity = new MapFromTestEntity
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Age = 30
        };

        // Act
        var target = new MapFromComputedTarget(entity);

        // Assert
        target.ShouldNotBeNull();
        target.Id.ShouldBe(1);
        target.DisplayName.ShouldBe("John"); // Mapped from FirstName
        target.Surname.ShouldBe("Doe"); // Mapped from LastName
        target.Email.ShouldBe("john@example.com");
        target.Age.ShouldBe(30);
    }

    [Fact]
    public void ToSource_ShouldNotIncludeComputedValues_WhenReversibleIsFalse()
    {
        // Arrange
        var target = new MapFromComputedTarget
        {
            Id = 2,
            DisplayName = "Jane", // Should NOT map back (Reversible = false by default)
            Surname = "Smith", // Should NOT map back (Reversible = false by default)
            Email = "jane@example.com",
            Age = 25
        };

        // Act
        var entity = target.ToSource();

        // Assert
        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(2);
        // FirstName and LastName should be default because mappings are not reversible
        entity.FirstName.ShouldBeEmpty();
        entity.LastName.ShouldBeEmpty();
        entity.Email.ShouldBe("jane@example.com");
        entity.Age.ShouldBe(25);
    }

    [Fact]
    public void Projection_ShouldMapComputedValues()
    {
        // Arrange
        var entities = new[]
        {
            new MapFromTestEntity { Id = 1, FirstName = "Alice", LastName = "Brown", Email = "alice@example.com", Age = 28 }
        }.AsQueryable();

        // Act
        var targets = entities.Select(MapFromComputedTarget.Projection).ToList();

        // Assert
        targets.Count().ShouldBe(1);
        targets[0].Id.ShouldBe(1);
        targets[0].DisplayName.ShouldBe("Alice");
        targets[0].Surname.ShouldBe("Brown");
    }

    [Fact]
    public void Constructor_ShouldMapExpressionToFullName()
    {
        // Arrange
        var entity = new MapFromTestEntity
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Age = 30
        };

        // Act
        var target = new MapFromExpressionTarget(entity);

        // Assert
        target.ShouldNotBeNull();
        target.Id.ShouldBe(1);
        target.FullName.ShouldBe("John Doe"); // Computed from FirstName + " " + LastName
        target.Email.ShouldBe("john@example.com");
        target.Age.ShouldBe(30);
    }

    [Fact]
    public void Projection_ShouldMapExpressionToFullName()
    {
        // Arrange
        var entities = new[]
        {
            new MapFromTestEntity { Id = 1, FirstName = "Alice", LastName = "Smith", Email = "alice@example.com", Age = 28 },
            new MapFromTestEntity { Id = 2, FirstName = "Bob", LastName = "Jones", Email = "bob@example.com", Age = 35 }
        }.AsQueryable();

        // Act
        var targets = entities.Select(MapFromExpressionTarget.Projection).ToList();

        // Assert
        targets.Count().ShouldBe(2);
        targets[0].FullName.ShouldBe("Alice Smith");
        targets[1].FullName.ShouldBe("Bob Jones");
    }

    // ========================================
    // Nested Property Path Tests
    // Testing [MapFrom("Property.Nested.Path")] with string literals
    // ========================================

    [Fact]
    public void Constructor_ShouldMapSingleLevelNestedPropertyPath()
    {
        // Arrange
        var entity = new MapFromNestedPropertyEntity
        {
            Id = 1,
            Name = "John Doe",
            Company = new MapFromCompanyEntity
            {
                Id = 100,
                CompanyName = "Acme Corporation",
                Address = "123 Main Street"
            }
        };

        // Act
        var target = new MapFromSingleLevelPathTarget(entity);

        // Assert
        target.ShouldNotBeNull();
        target.Id.ShouldBe(1);
        target.Name.ShouldBe("John Doe");
        target.CompanyName.ShouldBe("Acme Corporation"); // From Company.CompanyName
        target.CompanyAddress.ShouldBe("123 Main Street"); // From Company.Address
        target.AlternativeCompanyName.ShouldBe("Acme Corporation"); // using nameof(@MapFromNestedPropertyEntity.Company.CompanyName)
    }

    [Fact]
    public void Projection_ShouldMapSingleLevelNestedPropertyPath()
    {
        // Arrange
        var entities = new[]
        {
            new MapFromNestedPropertyEntity
            {
                Id = 1,
                Name = "Alice",
                Company = new MapFromCompanyEntity
                {
                    Id = 100,
                    CompanyName = "TechCorp",
                    Address = "456 Tech Ave"
                }
            },
            new MapFromNestedPropertyEntity
            {
                Id = 2,
                Name = "Bob",
                Company = new MapFromCompanyEntity
                {
                    Id = 200,
                    CompanyName = "StartupInc",
                    Address = "789 Innovation Blvd"
                }
            }
        }.AsQueryable();

        // Act
        var targets = entities.Select(MapFromSingleLevelPathTarget.Projection).ToList();

        // Assert
        targets.Count().ShouldBe(2);
        targets[0].CompanyName.ShouldBe("TechCorp");
        targets[0].CompanyAddress.ShouldBe("456 Tech Ave");
        targets[1].CompanyName.ShouldBe("StartupInc");
        targets[1].CompanyAddress.ShouldBe("789 Innovation Blvd");
    }

    [Fact]
    public void Constructor_ShouldMapMultiLevelNestedPropertyPath()
    {
        // Arrange
        var entity = new MapFromMultiLevelEntity
        {
            Id = 1,
            Employee = new MapFromNestedPropertyEntity
            {
                Id = 50,
                Name = "Jane Smith",
                Company = new MapFromCompanyEntity
                {
                    Id = 100,
                    CompanyName = "Global Enterprises",
                    Address = "999 Corporate Plaza"
                }
            }
        };

        // Act
        var target = new MapFromMultiLevelPathTarget(entity);

        // Assert
        target.ShouldNotBeNull();
        target.Id.ShouldBe(1);
        target.EmployeeName.ShouldBe("Jane Smith"); // From Employee.Name
        target.EmployeeCompanyName.ShouldBe("Global Enterprises"); // From Employee.Company.CompanyName
        target.EmployeeCompanyAddress.ShouldBe("999 Corporate Plaza"); // From Employee.Company.Address
    }

    [Fact]
    public void Projection_ShouldMapMultiLevelNestedPropertyPath()
    {
        // Arrange
        var entities = new[]
        {
            new MapFromMultiLevelEntity
            {
                Id = 1,
                Employee = new MapFromNestedPropertyEntity
                {
                    Id = 10,
                    Name = "Alice Johnson",
                    Company = new MapFromCompanyEntity
                    {
                        Id = 100,
                        CompanyName = "MegaCorp",
                        Address = "100 Business Park"
                    }
                }
            },
            new MapFromMultiLevelEntity
            {
                Id = 2,
                Employee = new MapFromNestedPropertyEntity
                {
                    Id = 20,
                    Name = "Bob Williams",
                    Company = new MapFromCompanyEntity
                    {
                        Id = 200,
                        CompanyName = "SmallBiz LLC",
                        Address = "200 Small St"
                    }
                }
            }
        }.AsQueryable();

        // Act
        var targets = entities.Select(MapFromMultiLevelPathTarget.Projection).ToList();

        // Assert
        targets.Count().ShouldBe(2);
        targets[0].EmployeeName.ShouldBe("Alice Johnson");
        targets[0].EmployeeCompanyName.ShouldBe("MegaCorp");
        targets[0].EmployeeCompanyAddress.ShouldBe("100 Business Park");
        targets[1].EmployeeName.ShouldBe("Bob Williams");
        targets[1].EmployeeCompanyName.ShouldBe("SmallBiz LLC");
        targets[1].EmployeeCompanyAddress.ShouldBe("200 Small St");
    }

    [Fact]
    public void Constructor_ShouldHandleNullIntermediatePropertyInNestedPath()
    {
        // Arrange
        var entity = new MapFromNestedPropertyEntity
        {
            Id = 1,
            Name = "John Doe",
            Company = null // Null company
        };

        // Act & Assert
        // This should throw NullReferenceException because Company is null
        // and the generated code doesn't include null checks for nested paths
        var action = () => new MapFromSingleLevelPathTarget(entity);
        action.ShouldThrow<NullReferenceException>();
    }

    [Fact]
    public void Projection_ShouldHandleNullIntermediatePropertyInNestedPath()
    {
        // Arrange
        var entities = new[]
        {
            new MapFromNestedPropertyEntity
            {
                Id = 1,
                Name = "Alice",
                Company = null // Null company
            }
        }.AsQueryable();

        // Act & Assert
        // EF Core projection with null nested path should also fail
        var action = () => entities.Select(MapFromSingleLevelPathTarget.Projection).ToList();
        action.ShouldThrow<NullReferenceException>();
    }

    [Fact]
    public void Constructor_NestedPropertyPath_ShouldNotGenerateDuplicateProperties()
    {
        // Verify that MapFrom with nested paths doesn't create duplicate properties
        var targetType = typeof(MapFromSingleLevelPathTarget);
        var properties = targetType.GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToList();

        // Should have our custom properties
        propertyNames.ShouldContain("CompanyName");
        propertyNames.ShouldContain("CompanyAddress");

        // Should have auto-generated properties from MapFromNestedPropertyEntity
        propertyNames.ShouldContain("Id");
        propertyNames.ShouldContain("Name");

        // Should NOT have the "Company" navigation property (it's mapped to nested paths)
        propertyNames.ShouldNotContain("Company");
    }
}
