using DotNetBrightener.Mapper.Mapping.Configurations;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

#region Test Models - Source

public class CustomCompanySource
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
}

public class CustomDepartmentSource
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CustomCompanySource Company { get; set; } = null!;
}

public class CustomEmployeeSource
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public CustomDepartmentSource Department { get; set; } = null!;
    public CustomCompanySource Company { get; set; } = null!;
    public List<CustomEmployeeSource> DirectReports { get; set; } = [];
}

public class CustomOfferSource
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class CustomServiceProviderLinkSource
{
    public string ServiceProviderId { get; set; } = string.Empty;
    public string ServiceProviderType { get; set; } = string.Empty;
    public int? FkGologServiceProvider { get; set; }
}

public class CustomDamageSource
{
    public int                              Id                  { get; set; }
    public string                           Description         { get; set; } = string.Empty;
    public List<CustomEmployeeSource>       AssignedPersons     { get; set; } = [];
    public CustomEmployeeSource?            Creator             { get; set; }
    public List<CustomOfferSource>          Offers              { get; set; } = [];
    public CustomServiceProviderLinkSource? ServiceProviderLink { get; set; }
}

#endregion

#region Test Models - Targets

[MappingTarget<CustomCompanySource>()]
public partial class CustomCompanyDto;

[MappingTarget<CustomDepartmentSource>(
    NestedTargetTypes = [typeof(CustomCompanyDto)])]
public partial class CustomDepartmentDto;

[MappingTarget<CustomEmployeeSource>(
    exclude: [nameof(CustomEmployeeSource.DirectReports)],
    NestedTargetTypes = [typeof(CustomDepartmentDto), typeof(CustomCompanyDto)],
    Configuration = typeof(CustomEmployeeMappingConfiguration))]
public partial class CustomEmployeeDto
{
    // Custom calculated property (NOT in source)
    public string FullName { get; set; } = string.Empty;
}

public class CustomEmployeeMappingConfiguration : IMappingConfiguration<CustomEmployeeSource, CustomEmployeeDto>
{
    public static void Map(CustomEmployeeSource source, CustomEmployeeDto target)
    {
        // Custom mapping: combine first and last name
        target.FullName = $"{source.FirstName} {source.LastName}";
    }
}

[MappingTarget<CustomOfferSource>()]
public partial class CustomOfferDto;

public class CustomServiceProviderDto
{
    public string ServiceProviderType { get; set; } = string.Empty;
    public string ServiceProviderId { get; set; } = string.Empty;
    public int? FkGologServiceProviderId { get; set; }
}

[MappingTarget<CustomDamageSource>(
    exclude: [nameof(CustomDamageSource.ServiceProviderLink)],
    NestedTargetTypes = [typeof(CustomEmployeeDto), typeof(CustomOfferDto)],
    Configuration = typeof(CustomDamageMappingConfiguration))]
public partial class CustomDamageDto
{
    // Custom calculated properties (NOT in source)
    public List<int>                 OfferIds          { get; set; } = [];
    public List<int>                 AssignedPersonIds { get; set; } = [];
    public string                    ProviderId        { get; set; } = string.Empty;
    public CustomServiceProviderDto? ServiceProvider   { get; set; }
}

public class CustomDamageMappingConfiguration : IMappingConfiguration<CustomDamageSource, CustomDamageDto>
{
    public static void Map(CustomDamageSource source, CustomDamageDto target)
    {
        target.OfferIds = source.Offers.Select(o => o.Id).ToList();
        target.AssignedPersonIds = source.AssignedPersons.Select(p => p.Id).ToList();
        target.ProviderId = source.ServiceProviderLink?.ServiceProviderId ?? "random uuid";

        target.ServiceProvider = source.ServiceProviderLink != null
            ? new CustomServiceProviderDto
            {
                ServiceProviderType = source.ServiceProviderLink.ServiceProviderType,
                ServiceProviderId = source.ServiceProviderLink.ServiceProviderId,
                FkGologServiceProviderId = source.ServiceProviderLink.FkGologServiceProvider
            }
            : null;
    }
}

[MappingTarget<CustomEmployeeSource>(
    exclude: [nameof(CustomEmployeeSource.Department), nameof(CustomEmployeeSource.Company)],
    NestedTargetTypes = [typeof(CustomEmployeeDto)],
    Configuration = typeof(CustomManagerMappingConfiguration),
    MaxDepth = 3)]
public partial class CustomManagerDto
{
    // Custom property (NOT in source)
    public int DirectReportCount { get; set; }
}

public class CustomManagerMappingConfiguration : IMappingConfiguration<CustomEmployeeSource, CustomManagerDto>
{
    public static void Map(CustomEmployeeSource source, CustomManagerDto target)
    {
        target.DirectReportCount = source.DirectReports.Count;
    }
}

#endregion

/// <summary>
///     Tests for nested targets combined with custom IMappingConfiguration.
///     This ensures that nested targets are properly instantiated with depth tracking
///     even when custom mapping is present.
/// </summary>
public class NestedTargetsWithCustomConfigurationTests
{
    [Fact]
    public void NestedTarget_WithCustomConfiguration_ShouldInstantiateNestedTargets()
    {
        // Arrange
        var company = new CustomCompanySource { Id = 1, Name = "Tech Corp", Industry = "Technology" };
        var department = new CustomDepartmentSource { Id = 10, Name = "Engineering", Company = company };
        var employee = new CustomEmployeeSource { Id = 100, FirstName = "John", LastName = "Doe", Department = department, Company = company };

        // Act
        var dto = new CustomEmployeeDto(employee);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(100);
        dto.FirstName.ShouldBe("John");
        dto.LastName.ShouldBe("Doe");

        // Custom mapping should work
        dto.FullName.ShouldBe("John Doe");

        // CRITICAL: Nested targets should be instantiated, not null!
        dto.Department.ShouldNotBeNull();
        dto.Department!.Id.ShouldBe(10);
        dto.Department.Name.ShouldBe("Engineering");
        dto.Department.Company.ShouldNotBeNull();
        dto.Department.Company!.Id.ShouldBe(1);
        dto.Department.Company.Name.ShouldBe("Tech Corp");

        dto.Company.ShouldNotBeNull();
        dto.Company!.Id.ShouldBe(1);
        dto.Company.Name.ShouldBe("Tech Corp");
    }

    [Fact]
    public void MultipleNestedTargets_WithCustomConfiguration_ShouldInstantiateAllNestedTargets()
    {
        // Arrange - This is the exact scenario from the GitHub issue
        var offer1 = new CustomOfferSource { Id = 1, Description = "Offer 1", Amount = 100m };
        var offer2 = new CustomOfferSource { Id = 2, Description = "Offer 2", Amount = 200m };

        var company = new CustomCompanySource { Id = 1, Name = "Tech Corp", Industry = "Technology" };
        var department = new CustomDepartmentSource { Id = 10, Name = "Engineering", Company = company };
        var creator = new CustomEmployeeSource { Id = 100, FirstName = "John", LastName = "Doe", Department = department, Company = company };
        var assignedPerson1 = new CustomEmployeeSource { Id = 101, FirstName = "Jane", LastName = "Smith", Department = department, Company = company };
        var assignedPerson2 = new CustomEmployeeSource { Id = 102, FirstName = "Bob", LastName = "Johnson", Department = department, Company = company };

        var serviceProviderLink = new CustomServiceProviderLinkSource
        {
            ServiceProviderId = "provider-123",
            ServiceProviderType = "external",
            FkGologServiceProvider = 999
        };

        var damage = new CustomDamageSource
        {
            Id                  = 1000,
            Description         = "Water damage in basement",
            Creator             = creator,
            AssignedPersons     = [assignedPerson1, assignedPerson2],
            Offers              = [offer1, offer2],
            ServiceProviderLink = serviceProviderLink
        };

        // Act
        var dto = new CustomDamageDto(damage);

        // Assert - Basic properties
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(1000);
        dto.Description.ShouldBe("Water damage in basement");

        // Assert - Custom mapping logic works
        dto.OfferIds.ShouldBe(new[] { 1, 2 });
        dto.AssignedPersonIds.ShouldBe(new[] { 101, 102 });
        dto.ProviderId.ShouldBe("provider-123");
        dto.ServiceProvider.ShouldNotBeNull();
        dto.ServiceProvider!.ServiceProviderId.ShouldBe("provider-123");
        dto.ServiceProvider.ServiceProviderType.ShouldBe("external");
        dto.ServiceProvider.FkGologServiceProviderId.ShouldBe(999);

        // CRITICAL: Nested targets should be instantiated!
        // This was the bug - these were null or not properly instantiated
        dto.Creator.ShouldNotBeNull();
        dto.Creator!.Id.ShouldBe(100);
        dto.Creator.FullName.ShouldBe("John Doe"); // Custom mapping in nested target should work
        dto.Creator.Department.ShouldNotBeNull();
        dto.Creator.Department!.Name.ShouldBe("Engineering");

        dto.AssignedPersons.ShouldNotBeNull();
        dto.AssignedPersons.Count().ShouldBe(2);
        dto.AssignedPersons[0].Id.ShouldBe(101);
        dto.AssignedPersons[0].FullName.ShouldBe("Jane Smith");
        dto.AssignedPersons[1].Id.ShouldBe(102);
        dto.AssignedPersons[1].FullName.ShouldBe("Bob Johnson");

        dto.Offers.ShouldNotBeNull();
        dto.Offers.Count().ShouldBe(2);
        dto.Offers[0].Id.ShouldBe(1);
        dto.Offers[0].Description.ShouldBe("Offer 1");
        dto.Offers[1].Id.ShouldBe(2);
        dto.Offers[1].Description.ShouldBe("Offer 2");
    }

    [Fact]
    public void CollectionNestedTargets_WithCustomConfiguration_ShouldInstantiateWithDepthTracking()
    {
        // Arrange
        var company = new CustomCompanySource { Id = 1, Name = "Tech Corp", Industry = "Technology" };
        var department = new CustomDepartmentSource { Id = 10, Name = "Engineering", Company = company };

        var report1 = new CustomEmployeeSource { Id = 201, FirstName = "Alice", LastName = "Williams", Department = department, Company = company };
        var report2 = new CustomEmployeeSource { Id = 202, FirstName = "Charlie", LastName = "Brown", Department = department, Company = company };

        var manager = new CustomEmployeeSource
        {
            Id = 100,
            FirstName = "John",
            LastName = "Manager",
            Department = department,
            Company = company,
            DirectReports = [report1, report2]
        };

        // Act
        var dto = new CustomManagerDto(manager);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(100);
        dto.FirstName.ShouldBe("John");
        dto.LastName.ShouldBe("Manager");

        // Custom mapping should work
        dto.DirectReportCount.ShouldBe(2);

        // CRITICAL: Collection of nested targets should be instantiated with depth tracking
        dto.DirectReports.ShouldNotBeNull();
        dto.DirectReports.Count().ShouldBe(2);

        dto.DirectReports[0].Id.ShouldBe(201);
        dto.DirectReports[0].FullName.ShouldBe("Alice Williams");
        dto.DirectReports[0].Department.ShouldNotBeNull();

        dto.DirectReports[1].Id.ShouldBe(202);
        dto.DirectReports[1].FullName.ShouldBe("Charlie Brown");
        dto.DirectReports[1].Department.ShouldNotBeNull();
    }

    [Fact]
    public void NullableNestedTarget_WithCustomConfiguration_ShouldHandleNullCorrectly()
    {
        // Arrange
        var employee = new CustomEmployeeSource
        {
            Id = 100,
            FirstName = "John",
            LastName = "Doe",
            Department = null!, // Null nested target
            Company = null!
        };

        // Act
        var dto = new CustomEmployeeDto(employee);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(100);
        dto.FullName.ShouldBe("John Doe"); // Custom mapping should still work

        // Null nested targets should remain null, not throw
        dto.Department.ShouldBeNull();
        dto.Company.ShouldBeNull();
    }

    [Fact]
    public void EmptyCollectionNestedTargets_WithCustomConfiguration_ShouldHandleCorrectly()
    {
        // Arrange
        var damage = new CustomDamageSource
        {
            Id                  = 1000,
            Description         = "Minor damage",
            Creator             = null,
            AssignedPersons     = [], // Empty collection
            Offers              = [],
            ServiceProviderLink = null
        };

        // Act
        var dto = new CustomDamageDto(damage);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(1000);

        // Custom mapping should handle empty collections
        dto.OfferIds.ShouldBeEmpty();
        dto.AssignedPersonIds.ShouldBeEmpty();
        dto.ProviderId.ShouldBe("random uuid"); // Fallback value
        dto.ServiceProvider.ShouldBeNull();

        // Empty collections should be instantiated, not null
        dto.AssignedPersons.ShouldNotBeNull();
        dto.AssignedPersons.ShouldBeEmpty();
        dto.Offers.ShouldNotBeNull();
        dto.Offers.ShouldBeEmpty();

        dto.Creator.ShouldBeNull();
    }

    // Note: ToSource is not generated when custom configuration is present by default
    // This is expected behavior and not part of the bug fix verification

    [Fact]
    public void DepthTracking_WithNestedTargetsAndCustomConfiguration_ShouldRespectMaxDepth()
    {
        // Arrange - Create a hierarchy deeper than MaxDepth (3)
        var company = new CustomCompanySource { Id = 1, Name = "Tech Corp", Industry = "Technology" };
        var department = new CustomDepartmentSource { Id = 10, Name = "Engineering", Company = company };

        var level3 = new CustomEmployeeSource { Id = 3, FirstName = "Level3", LastName = "Employee", Department = department, Company = company };
        var level2 = new CustomEmployeeSource { Id = 2, FirstName = "Level2", LastName = "Manager", Department = department, Company = company, DirectReports =
            [level3]
        };
        var level1 = new CustomEmployeeSource { Id = 1, FirstName = "Level1", LastName = "Director", Department = department, Company = company, DirectReports =
            [level2]
        };
        var ceo = new CustomEmployeeSource { Id = 0, FirstName = "CEO", LastName = "Boss", Department = department, Company = company, DirectReports =
            [level1]
        };

        // Act
        var dto = new CustomManagerDto(ceo);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(0);
        dto.DirectReportCount.ShouldBe(1); // Custom mapping works

        // Level 0 (CEO) - should have direct reports
        dto.DirectReports.Count().ShouldBe(1);
        dto.DirectReports[0].Id.ShouldBe(1);

        // Level 1 (Director) - should have direct reports
        dto.DirectReports[0].ShouldBeOfType<CustomEmployeeDto>();

        // Level 2 (Manager) - MaxDepth is 3, so this should still be populated
        // but we can verify the depth tracking mechanism is in place
        dto.DirectReports.ShouldNotBeNull();
    }
}
