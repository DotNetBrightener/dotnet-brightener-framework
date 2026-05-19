namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

/// <summary>
///     Tests for GitHub issue #249: Generator issue when DTOs with the same name are under different namespaces.
///     Verifies that types with the same simple name in different namespaces generate correctly.
/// </summary>
public class SameNameDifferentNamespaceTests
{
    [Fact]
    public void TargetWithSameName_InDifferentNamespaces_ShouldGenerateSeparateTypes()
    {
        // Arrange & Act - Just accessing the types proves they were generated successfully
        var n1DtoType = typeof(global::DotNetBrightener.Mapper.Tests.TestModels.SameName.A.EmployeeDto);
        var n2DtoType = typeof(global::DotNetBrightener.Mapper.Tests.TestModels.SameName.B.EmployeeDto);

        // Assert - Both types should exist and be different
        n1DtoType.ShouldNotBeNull();
        n2DtoType.ShouldNotBeNull();
        n1DtoType.ShouldNotBeSameAs(n2DtoType);
        n1DtoType.FullName.ShouldBe("DotNetBrightener.Mapper.Tests.TestModels.SameName.A.EmployeeDto");
        n2DtoType.FullName.ShouldBe("DotNetBrightener.Mapper.Tests.TestModels.SameName.B.EmployeeDto");
    }

    [Fact]
    public void TargetWithSameName_N1_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var dtoType = typeof(global::DotNetBrightener.Mapper.Tests.TestModels.SameName.A.EmployeeDto);

        // Assert - N1 Employee has Salary and Department
        dtoType.GetProperty("Salary").ShouldNotBeNull();
        dtoType.GetProperty("Salary")!.PropertyType.ShouldBe(typeof(decimal));
        
        dtoType.GetProperty("Department").ShouldNotBeNull();
        dtoType.GetProperty("Department")!.PropertyType.ShouldBe(typeof(string));
        
        // Should not have Role (that's in N2)
        dtoType.GetProperty("Role").ShouldBeNull();
    }

    [Fact]
    public void TargetWithSameName_N2_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var dtoType = typeof(global::DotNetBrightener.Mapper.Tests.TestModels.SameName.B.EmployeeDto);

        // Assert - N2 Employee has Salary and Role
        dtoType.GetProperty("Salary").ShouldNotBeNull();
        dtoType.GetProperty("Salary")!.PropertyType.ShouldBe(typeof(decimal));
        
        dtoType.GetProperty("Role").ShouldNotBeNull();
        dtoType.GetProperty("Role")!.PropertyType.ShouldBe(typeof(string));
        
        // Should not have Department (that's in N1)
        dtoType.GetProperty("Department").ShouldBeNull();
    }

    [Fact]
    public void TargetWithSameName_N1_ConstructorFromSource_ShouldWork()
    {
        // Arrange
        var source = new global::DotNetBrightener.Mapper.Tests.TestModels.SameName.A.Employee
        {
            Salary = 75000m,
            Department = "Engineering"
        };

        // Act
        var dto = new global::DotNetBrightener.Mapper.Tests.TestModels.SameName.A.EmployeeDto(source);

        // Assert
        dto.Salary.ShouldBe(75000m);
        dto.Department.ShouldBe("Engineering");
    }

    [Fact]
    public void TargetWithSameName_N2_ConstructorFromSource_ShouldWork()
    {
        // Arrange
        var source = new global::DotNetBrightener.Mapper.Tests.TestModels.SameName.B.Employee
        {
            Salary = 85000m,
            Role = "Senior Developer"
        };

        // Act
        var dto = new global::DotNetBrightener.Mapper.Tests.TestModels.SameName.B.EmployeeDto(source);

        // Assert
        dto.Salary.ShouldBe(85000m);
        dto.Role.ShouldBe("Senior Developer");
    }
}
