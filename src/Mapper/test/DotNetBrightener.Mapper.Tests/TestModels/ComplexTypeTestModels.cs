namespace DotNetBrightener.Mapper.Tests.TestModels;

// Source entities for complex type testing
public class AddressEntity
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class CompanyEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public AddressEntity HeadquartersAddress { get; set; } = new();
}

public class StaffMember
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public CompanyEntity Company { get; set; } = new();
    public AddressEntity HomeAddress { get; set; } = new();
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
}

public class DepartmentEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CompanyEntity Company { get; set; } = new();
    public StaffMember Manager { get; set; } = new();
    public int EmployeeCount { get; set; }
}

// Target DTOs
[MappingTarget<AddressEntity>( GenerateToSource = true)]
public partial record AddressTarget;

[MappingTarget<CompanyEntity>(
    NestedTargetTypes = [typeof(AddressTarget)],
    GenerateToSource = true)]
public partial record CompanyTarget;

[MappingTarget<StaffMember>(
    "PasswordHash", "Salary",
    NestedTargetTypes = [typeof(CompanyTarget), typeof(AddressTarget)],
    GenerateToSource = true)]
public partial record StaffMemberTarget;

[MappingTarget<DepartmentEntity>( NestedTargetTypes = [typeof(CompanyTarget), typeof(StaffMemberTarget)], GenerateToSource = true)]
public partial record DepartmentTarget;

// Collection nested targets test models
public class OrderItemEntity
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class OrderEntity
{
    public int                   Id              { get; set; }
    public string                OrderNumber     { get; set; } = string.Empty;
    public DateTime              OrderDate       { get; set; }
    public List<OrderItemEntity> Items           { get; set; } = [];
    public AddressEntity         ShippingAddress { get; set; } = new();
}

public class TeamEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public StaffMember[] Members { get; set; } = [];
}

public class ProjectEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<TeamEntity> Teams { get; set; } = new List<TeamEntity>();
}

// Collection nested target DTOs
[MappingTarget<OrderItemEntity>( GenerateToSource = true)]
public partial record OrderItemTarget;

[MappingTarget<OrderEntity>( NestedTargetTypes = [typeof(OrderItemTarget), typeof(AddressTarget)], GenerateToSource = true)]
public partial record OrderTarget;

[MappingTarget<TeamEntity>( NestedTargetTypes = [typeof(StaffMemberTarget)], GenerateToSource = true)]
public partial record TeamTarget;

[MappingTarget<ProjectEntity>( NestedTargetTypes = [typeof(TeamTarget)], GenerateToSource = true)]
public partial record ProjectTarget;

// IReadOnlyList and IReadOnlyCollection test models
public class LibraryBookEntity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
}

public class LibraryEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IReadOnlyList<LibraryBookEntity> Books { get; set; } = new List<LibraryBookEntity>();
    public IReadOnlyCollection<StaffMember> Staff { get; set; } = new List<StaffMember>();
}

// IReadOnlyList and IReadOnlyCollection target DTOs
[MappingTarget<LibraryBookEntity>( GenerateToSource = true)]
public partial record LibraryBookTarget;

[MappingTarget<LibraryEntity>( NestedTargetTypes = [typeof(LibraryBookTarget), typeof(StaffMemberTarget)], GenerateToSource = true)]
public partial record LibraryTarget;

// Test models from GitHub issue #218
public class BobChild
{
    public required string Name { get; set; }
}

public class Bob
{
    public IReadOnlyList<BobChild> ReadOnlyRelationships { get; set; } = [];
    public List<BobChild> Relationships { get; set; } = [];
}

[MappingTarget<BobChild>()]
public partial record BobChildModel;

[MappingTarget<Bob>( NestedTargetTypes = [typeof(BobChildModel)])]
public partial record BobModel;

// Test models for GitHub issue #220 - Smarter GenerateToSource
public class MyClass
{
    public string Name { get; private set; }

    internal MyClass(string name) => Name = name;

    public static MyClass Create(string name) => new(name);
}

[MappingTarget<MyClass>( GenerateToSource = true)]
public partial record MyClassModel;
