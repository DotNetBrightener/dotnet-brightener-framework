using System.Reflection;
using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Tests.TestModels;
using DotNetBrightener.Mapper.Tests.Utilities;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Features;

public class InheritanceTests
{
    [Fact]
    public void ToTarget_ShouldMapEmployeeProperties_IncludingInheritedFromUser()
    {
        // Arrange
        var employee = TestDataFactory.CreateEmployee("Jane", "Smith", "Engineering");

        // Act
        var dto = employee.ToTarget<Employee, EmployeeDto>();

        // Assert
        ShouldBeNullExtensions.ShouldNotBeNull<EmployeeDto>(dto);
        
        // Inherited properties from User
        dto.Id.ShouldBe(employee.Id);
        dto.FirstName.ShouldBe("Jane");
        dto.LastName.ShouldBe("Smith");
        dto.Email.ShouldBe(employee.Email);
        dto.DateOfBirth.ShouldBe(employee.DateOfBirth);
        dto.IsActive.ShouldBeTrue();
        dto.LastLoginAt.ShouldBe(employee.LastLoginAt);
        
        // Employee-specific properties
        dto.EmployeeId.ShouldBe(employee.EmployeeId);
        dto.Department.ShouldBe("Engineering");
        dto.HireDate.ShouldBe(employee.HireDate);
    }

    [Fact]
    public void ToTarget_ShouldExcludeSpecifiedProperties_FromEmployeeMapping()
    {
        // Arrange
        var employee = TestDataFactory.CreateEmployee();

        // Act
        var dto = employee.ToTarget<Employee, EmployeeDto>();

        // Assert
        var dtoType = dto.GetType();
        ShouldBeNullExtensions.ShouldBeNull<PropertyInfo>(dtoType.GetProperty("Password"), "Password should be excluded");
        ShouldBeNullExtensions.ShouldBeNull<PropertyInfo>(dtoType.GetProperty("Salary"), "Salary should be excluded");
        ShouldBeNullExtensions.ShouldBeNull<PropertyInfo>(dtoType.GetProperty("CreatedAt"), "CreatedAt should be excluded");
    }

    [Fact]
    public void ToTarget_ShouldMapManagerProperties_IncludingMultipleLevelsOfInheritance()
    {
        // Arrange
        var manager = TestDataFactory.CreateManager("Mike", "Johnson", "Development Team");

        // Act
        var dto = manager.ToTarget<Manager, ManagerDto>();

        // Assert
        dto.ShouldNotBeNull();
        
        // Inherited from User
        dto.Id.ShouldBe(manager.Id);
        dto.FirstName.ShouldBe("Mike");
        dto.LastName.ShouldBe("Johnson");
        dto.Email.ShouldBe(manager.Email);
        dto.IsActive.ShouldBeTrue();
        
        // Inherited from Employee
        dto.EmployeeId.ShouldBe(manager.EmployeeId);
        dto.Department.ShouldBe("Engineering");
        dto.HireDate.ShouldBe(manager.HireDate);
        
        // Manager-specific properties
        dto.TeamName.ShouldBe("Development Team");
        dto.TeamSize.ShouldBe(8);
    }

    [Fact]
    public void ToTarget_ShouldExcludeMultipleProperties_FromManagerMapping()
    {
        // Arrange
        var manager = TestDataFactory.CreateManager();

        // Act
        var dto = manager.ToTarget<Manager, ManagerDto>();

        // Assert
        var dtoType = dto.GetType();
        dtoType.GetProperty("Password").ShouldBeNull("Password should be excluded");
        dtoType.GetProperty("Salary").ShouldBeNull("Salary should be excluded");
        dtoType.GetProperty("Budget").ShouldBeNull("Budget should be excluded");
        dtoType.GetProperty("CreatedAt").ShouldBeNull("CreatedAt should be excluded");
    }

    [Fact]
    public void ToTarget_ShouldHandlePolymorphism_WhenMappingDerivedTypes()
    {
        // Arrange
        var baseUser = TestDataFactory.CreateUser("Base", "User");
        var employee = TestDataFactory.CreateEmployee("Employee", "User");
        var manager = TestDataFactory.CreateManager("Manager", "User");

        // Act
        var baseDto = baseUser.ToTarget<User, UserDto>();
        var employeeDto = employee.ToTarget<Employee, EmployeeDto>();
        var managerDto = manager.ToTarget<Manager, ManagerDto>();

        // Assert
        baseDto.FirstName.ShouldBe("Base");
        employeeDto.FirstName.ShouldBe("Employee");
        managerDto.FirstName.ShouldBe("Manager");
        
        // Each should have their specific properties
        baseDto.GetType().GetProperty("EmployeeId").ShouldBeNull();
        ShouldBeNullExtensions.ShouldNotBeNull<PropertyInfo>(employeeDto.GetType().GetProperty("EmployeeId"));
        managerDto.GetType().GetProperty("TeamName").ShouldNotBeNull();
    }

    [Fact]
    public void ToTarget_ShouldExcludeInheritedProperty_FromGenericBaseClass()
    {
        // Arrange - tests that we can exclude Id inherited from BaseEntity<uint>
        var category = new Category
        {
            Id = 42,
            Name = "Electronics",
            Description = "Electronic devices and accessories"
        };

        // Act
        var dto = category.ToTarget<Category, UpdateCategoryViewModel>();

        // Assert
        dto.ShouldNotBeNull();
        dto.Name.ShouldBe("Electronics");
        dto.Description.ShouldBe("Electronic devices and accessories");

        // The Id property should NOT exist in the DTO (it was excluded)
        var dtoType = dto.GetType();
        dtoType.GetProperty("Id").ShouldBeNull("Id should be excluded from UpdateCategoryViewModel");
    }
}
