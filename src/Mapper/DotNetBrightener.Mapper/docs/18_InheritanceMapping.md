# Inheritance Mapping

`DotNetBrightener.Mapper` fully supports inheritance hierarchies in both source types and target types. This guide covers how to work with inherited properties, base classes, and polymorphic scenarios.

## How Inheritance Works in MappingTarget Generation

When you create a target type from a source type that has a base class, `DotNetBrightener.Mapper` automatically includes all inherited properties from the entire inheritance chain. Similarly, your target types can inherit from base classes to share common properties.

## Mapping Inherited Properties

### Source Type with Inheritance

```csharp
// Base domain model
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }  // Sensitive
    public DateTime DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Derived domain model
public class Employee : User
{
    public string EmployeeId { get; set; }
    public string Department { get; set; }
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }  // Sensitive
}

// Further derived
public class Manager : Employee
{
    public string TeamName { get; set; }
    public int TeamSize { get; set; }
    public decimal Budget { get; set; }  // Sensitive
}
```

### Creating Target Types for Derived Types

When you create a target type for `Employee`, it automatically includes properties from `User`:

```csharp
// Target type for Employee - excludes sensitive properties from all levels
[MappingTarget<Employee>("Password", "Salary", "CreatedAt")]
public partial class EmployeeDto;

// Generated properties include:
// From User: Id, FirstName, LastName, Email, DateOfBirth, IsActive
// From Employee: EmployeeId, Department, HireDate
// Excluded: Password (User), Salary (Employee), CreatedAt (User)
```

For `Manager`, exclude sensitive properties from all inheritance levels:

```csharp
[MappingTarget<Manager>("Password", "Salary", "Budget", "CreatedAt")]
public partial class ManagerDto;

// Generated properties include:
// From User: Id, FirstName, LastName, Email, DateOfBirth, IsActive
// From Employee: EmployeeId, Department, HireDate
// From Manager: TeamName, TeamSize
// Excluded: Password, Salary, Budget, CreatedAt
```

## Target Types with Base Classes

Your target types can also use inheritance to share common properties and avoid duplication.

### Defining Base Target Classes

```csharp
// Abstract base class for shared properties
public abstract class BaseTarget
{
    public int Id { get; set; }
    public bool IsActive { get; set; }
}

// Or with common audit properties
public abstract class AuditableTarget : BaseTarget
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### Target Types Inheriting from Base Classes

When your target type inherits from a base class, `DotNetBrightener.Mapper` automatically detects inherited properties and won't generate duplicates:

```csharp
// Entity with all properties
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public string InternalCode { get; set; }  // Internal only
}

// Base target class
public abstract class BaseTarget
{
    public int Id { get; set; }
    public bool IsActive { get; set; }
}

// Target type that inherits from base - Id and IsActive come from base class
[MappingTarget<Product>("InternalCode")]
public partial class ProductDto : BaseTarget
{
    // Generated properties: Name, Description, Price
    // Inherited from BaseTarget: Id, IsActive (NOT duplicated)
}
```

### Verification

You can verify that properties aren't duplicated:

```csharp
var facetType = typeof(ProductDto);
var declaredProperties = facetType.GetProperties(
    BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

// Id and IsActive should NOT be in declared properties (they're inherited)
var propertyNames = declaredProperties.Select(p => p.Name).ToList();
Assert.DoesNotContain("Id", propertyNames);
Assert.DoesNotContain("IsActive", propertyNames);

// Name, Description, Price should be declared
Assert.Contains("Name", propertyNames);
Assert.Contains("Description", propertyNames);
Assert.Contains("Price", propertyNames);
```

## Generic Base Classes

`DotNetBrightener.Mapper` also handles generic base classes correctly:

```csharp
// Generic base entity
public class BaseEntity<TKey>
{
    public TKey Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// Concrete entity
public class Category : BaseEntity<uint>
{
    public string Name { get; set; }
    public string Description { get; set; }
}

// Target type excluding Id (inherited from generic base)
[MappingTarget<Category>("Id")]
public partial class UpdateCategoryDto;

// Result: Name, Description, CreatedAt, UpdatedAt (Id excluded)
```

## Polymorphic Mapping

For polymorphic scenarios where you have a collection of base types containing derived instances:

```csharp
// Map each type to its specific target type
var users = GetAllUsers(); // Returns User, Employee, and Manager instances

// Option 1: Map all to base target type
var userDtos = users.Select(u => u.ToTarget<User, UserDto>()).ToList();

// Option 2: Use pattern matching for type-specific target types
var results = users.Select(u => u switch
{
    Manager m => (object)m.ToTarget<Manager, ManagerDto>(),
    Employee e => (object)e.ToTarget<Employee, EmployeeDto>(),
    User user => (object)user.ToTarget<User, UserDto>()
}).ToList();
```

## Include Mode with Inheritance

When using `Include` mode with inherited properties, you can include properties from any level of the inheritance hierarchy:

```csharp
// Include specific properties from both User and Employee
[MappingTarget<Employee>(Include = [
        nameof(User.Id), 
        nameof(User.FirstName), 
        nameof(User.LastName),
        nameof(Employee.Department),
        nameof(Employee.HireDate)
    ])]
public partial class EmployeeBasicDto;
```

## Constructor and Projection Behavior

When mapping derived types, the generated constructor and projection properly access all inherited properties:

```csharp
// Generated constructor (simplified)
public EmployeeDto(Employee source)
{
    // Properties from User (base class)
    this.Id = source.Id;
    this.FirstName = source.FirstName;
    this.LastName = source.LastName;
    this.Email = source.Email;
    this.DateOfBirth = source.DateOfBirth;
    this.IsActive = source.IsActive;
    
    // Properties from Employee
    this.EmployeeId = source.EmployeeId;
    this.Department = source.Department;
    this.HireDate = source.HireDate;
}

// Generated projection
public static Expression<Func<Employee, EmployeeDto>> Projection =>
    source => new EmployeeDto
    {
        Id = source.Id,
        FirstName = source.FirstName,
        // ... all properties including inherited
    };
```

## Best Practices

### 1. Exclude Sensitive Properties at Each Level

When creating target types for derived types, remember to exclude sensitive properties from all levels of the hierarchy:

```csharp
// Good: Exclude from all levels
[MappingTarget<Manager>("Password", "Salary", "Budget")]
public partial class ManagerDto;

// Avoid: Forgetting base class sensitive properties
[MappingTarget<Manager>("Budget")]  // Password and Salary still exposed!
public partial class ManagerDto;
```

### 2. Use Base Target Classes for Consistency

```csharp
// Define base target type with common properties
public abstract class BaseUserTarget
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsActive { get; set; }
}

// All user-related target types inherit from base
[MappingTarget<User>("Password", "CreatedAt")]
public partial class UserDto : BaseUserTarget;

[MappingTarget<Employee>("Password", "Salary", "CreatedAt")]
public partial class EmployeeDto : BaseUserTarget;
```

### 3. Consider Separate Target Types for Different Purposes

```csharp
// Public API response - minimal data
[MappingTarget<Employee>(Include = ["Id", "FirstName", "LastName", "Department"])]
public partial class EmployeePublicDto;

// Internal/Admin response - more data but still no passwords
[MappingTarget<Employee>("Password")]
public partial class EmployeeAdminDto;

// HR response - includes salary
[MappingTarget<Employee>("Password")]
public partial class EmployeeHrDto;
```

## Limitations

1. **Abstract Source Types**: You cannot create a target type directly from an abstract class because there are no instances to map. Create target types for concrete derived types instead.

2. **Interface Source Types**: `DotNetBrightener.Mapper` works with classes and structs, not interfaces. Define target types for implementing classes.

3. **Multiple Inheritance**: C# doesn't support multiple inheritance, so target types can only inherit from a single base class (plus interfaces).

## See Also

- [Basic Mapping](02_QuickStart.md)
- [Custom Mapping](04_CustomMapping.md)
- [What is Being Generated](07_WhatIsBeingGenerated.md)
