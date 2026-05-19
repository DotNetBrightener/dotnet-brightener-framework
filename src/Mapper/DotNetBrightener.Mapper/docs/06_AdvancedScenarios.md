# Advanced Scenarios

This section covers advanced use cases and configuration options for `DotNetBrightener.Mapper`.

## Multiple Target DTOs from One Source

You can create multiple target DTOs from the same source type:

```csharp
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Password { get; set; }
    public decimal Salary { get; set; }
    public string Department { get; set; }
}

// Public profile (exclude sensitive data)
[MappingTarget<User>(nameof(User.Password), nameof(User.Salary))]
public partial class UserPublicDto { }

// Contact information only (include specific fields)
[MappingTarget<User>(Include = [nameof(User.FirstName), nameof(User.LastName), nameof(User.Email)])]
public partial class UserContactDto { }

// Summary for lists (include minimal data)
[MappingTarget<User>(Include = [nameof(User.Id), nameof(User.FirstName), nameof(User.LastName)])]
public partial class UserSummaryDto { }

// HR view (exclude password but include salary)
[MappingTarget<User>(nameof(User.Password))]
public partial class UserHRDto { }
```

## Include vs Exclude Patterns

### Include Pattern - Building Focused DTOs

Use the `Include` pattern when you want target DTOs with only specific properties:

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
    public string InternalNotes { get; set; }
    public decimal Cost { get; set; }
    public string SKU { get; set; }
}

// API response with only customer-facing data
[MappingTarget<Product>(Include = [nameof(Product.Id), nameof(Product.Name), nameof(Product.Description), nameof(Product.Price), nameof(Product.IsAvailable)])]
public partial record ProductApiDto;

// Search results with minimal data
[MappingTarget<Product>(Include = [nameof(Product.Id), nameof(Product.Name), nameof(Product.Price)])]
public partial record ProductSearchDto;

// Internal admin view with cost data
[MappingTarget<Product>(Include = [nameof(Product.Id), nameof(Product.Name), nameof(Product.Price), nameof(Product.Cost), nameof(Product.SKU), nameof(Product.InternalNotes)])]
public partial class ProductAdminDto;
```

### Exclude Pattern - Hiding Sensitive Data

Use the `Exclude` pattern when you want most properties but need to hide specific ones:

```csharp
// Exclude only sensitive information
[MappingTarget<User>(nameof(User.Password))]
public partial class UserDto { }

// Exclude multiple sensitive fields
[MappingTarget<Employee>(nameof(Employee.Salary), nameof(Employee.SSN))]
public partial class EmployeePublicDto { }
```

## Working with Fields

### Include Fields Example

```csharp
public class LegacyEntity
{
    public int Id;
    public string Name;
    public DateTime CreatedDate;
    public string Status { get; set; }
    public string Notes { get; set; }
}

// Include specific fields and properties
[MappingTarget<LegacyEntity>(Include = [nameof(LegacyEntity.Name), nameof(LegacyEntity.Status)], IncludeFields = true)]
public partial class LegacyEntityDto;

// Only include properties (fields ignored even if listed)
[MappingTarget<LegacyEntity>(Include = [nameof(LegacyEntity.Status), nameof(LegacyEntity.Notes), nameof(LegacyEntity.Name)], IncludeFields = false)]
public partial class LegacyEntityPropsOnlyDto;
```

## Nested Target Types - Composing DTOs

`DotNetBrightener.Mapper` supports automatic mapping of nested objects through the `NestedTargetTypes` parameter. This eliminates the need to manually declare nested properties and handle their mapping.

### Basic Nested Target Types

```csharp
// Source entities with nested structure
public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
}

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Address HeadquartersAddress { get; set; }
}

// DTOs with automatic nested target mapping
[MappingTarget<Address>]
public partial record AddressDto;

[MappingTarget<Company>(NestedTargetTypes = [typeof(AddressDto)])]
public partial record CompanyDto;

// Usage
var company = new Company
{
    Name = "Acme Corp",
    HeadquartersAddress = new Address { City = "San Francisco" }
};

var companyDto = new CompanyDto(company);
// companyDto.HeadquartersAddress is automatically an AddressDto
```

### Multi-Level Nesting

```csharp
public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PasswordHash { get; set; }
    public decimal Salary { get; set; }
    public Company Company { get; set; }
    public Address HomeAddress { get; set; }
}

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Company Company { get; set; }
    public Employee Manager { get; set; }
}

// Employee DTO with multiple nested target types
[MappingTarget<Employee>("PasswordHash", "Salary",
    NestedTargetTypes = [typeof(CompanyDto), typeof(AddressDto)])]
public partial record EmployeeDto;

// Department DTO with deeply nested structure
[MappingTarget<Department>(NestedTargetTypes = [typeof(CompanyDto), typeof(EmployeeDto)])]
public partial record DepartmentDto;

// Usage - handles 3+ levels of nesting automatically
var department = new Department
{
    Name = "Engineering",
    Company = new Company
    {
        Name = "Tech Corp",
        HeadquartersAddress = new Address { City = "Seattle" }
    },
    Manager = new Employee
    {
        FirstName = "Jane",
        Company = new Company
        {
            Name = "Tech Corp",
            HeadquartersAddress = new Address { City = "Seattle" }
        },
        HomeAddress = new Address { City = "Bellevue" }
    }
};

var departmentDto = new DepartmentDto(department);
// departmentDto.Manager.Company.HeadquartersAddress.City == "Seattle"
```

### How NestedTargetTypes Works

**Automatic Type Detection:**
- The generator inspects each nested target type's source type
- Properties in the parent source that match nested target source types are automatically replaced
- For example, if `CompanyDto` maps from `Company`, any `Company` property becomes `CompanyDto`

**Generated Code:**
```csharp
// For: [MappingTarget<Company>(NestedTargetTypes = [typeof(AddressDto)])]
public partial record CompanyDto
{
    public int Id { get; init; }
    public string Name { get; init; }
    public AddressDto HeadquartersAddress { get; init; } // Automatically becomes AddressDto

    public CompanyDto(Company source)
        : this(source.Id, source.Name, new AddressDto(source.HeadquartersAddress)) // Automatic nesting
    { }

    public Company ToSource()
    {
        return new Company
        {
            Id = this.Id,
            Name = this.Name,
            HeadquartersAddress = this.HeadquartersAddress.ToSource() // Automatic reverse mapping
        };
    }
}
```

### EF Core Projections with Nested Target Types

```csharp
// Works seamlessly with Entity Framework Core
var companies = await dbContext.Companies
    .Where(c => c.IsActive)
    .Select(CompanyDto.Projection)
    .ToListAsync();

// The generated projection handles nested types automatically:
// c => new CompanyDto(c.Id, c.Name, new AddressDto(c.HeadquartersAddress))
```

### Benefits

1. **No Manual Property Declarations**: Don't redeclare nested properties
2. **Automatic Constructor Mapping**: Nested constructors are called automatically
3. **ToSource Support**: Reverse mapping works for nested structures
4. **EF Core Compatible**: Projections work in database queries
5. **Multi-Level Support**: Handle 3+ levels of nesting

## Collection Nested Target Types - Working with Lists and Arrays

`DotNetBrightener.Mapper` fully supports nested target types within collections, automatically mapping `List<T>`, `ICollection<T>`, `T[]`, and other collection types to their corresponding nested target types.

### Basic Collection Mapping

```csharp
// Source entities
public class OrderItem
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public List<OrderItem> Items { get; set; }  // Collection of nested objects
}

// DTOs
[MappingTarget<OrderItem>]
public partial record OrderItemDto;

[MappingTarget<Order>(NestedTargetTypes = [typeof(OrderItemDto)])]
public partial record OrderDto;

// Usage
var order = new Order
{
    Id = 1,
    OrderNumber = "ORD-2025-001",
    OrderDate = DateTime.Now,
    Items = new List<OrderItem>
    {
        new() { Id = 1, ProductName = "Laptop", Price = 1200.00m, Quantity = 1 },
        new() { Id = 2, ProductName = "Mouse", Price = 25.00m, Quantity = 2 }
    }
};

var orderDto = new OrderDto(order);
// orderDto.Items is List<OrderItemDto>
// Each OrderItem is automatically mapped to OrderItemDto
```

### Supported Collection Types

`DotNetBrightener.Mapper` automatically handles all common collection types:

```csharp
public class Project
{
    // All of these work with NestedTargetTypes:
    public List<Task> Tasks { get; set; }              // List<T>
    public ICollection<Team> Teams { get; set; }       // ICollection<T>
    public IList<Milestone> Milestones { get; set; }   // IList<T>
    public IEnumerable<Comment> Comments { get; set; } // IEnumerable<T>
    public Employee[] Employees { get; set; }          // T[] (arrays)
}

[MappingTarget<Project>(NestedTargetTypes = [typeof(TaskDto), typeof(TeamDto), /* ... */])]
public partial record ProjectDto;
// All collections automatically map to their corresponding DTO collection types:
// - List<Task> → List<TaskDto>
// - ICollection<Team> → ICollection<TeamDto> (implemented as List)
// - Employee[] → EmployeeDto[]
```

### Generated Code for Collections

The generator creates efficient LINQ-based transformations:

```csharp
// Generated constructor
public OrderDto(Order source)
{
    Id = source.Id;
    OrderNumber = source.OrderNumber;
    OrderDate = source.OrderDate;

    // Uses LINQ Select to map each element
    Items = source.Items.Select(x => new OrderItemDto(x)).ToList();
}

// Generated ToSource method
public Order ToSource()
{
    return new Order
    {
        Id = this.Id,
        OrderNumber = this.OrderNumber,
        OrderDate = this.OrderDate,

        // Maps each DTO back to entity
        Items = this.Items.Select(x => x.ToSource()).ToList()
    };
}
```

### Multi-Level Collection Nesting

Collections can be nested at any depth:

```csharp
public class OrderItem
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public List<OrderItemOption> Options { get; set; }  // Nested collection
}

public class Order
{
    public int Id { get; set; }
    public List<OrderItem> Items { get; set; }  // Collection of objects with collections
}

[MappingTarget<OrderItemOption>]
public partial record OrderItemOptionDto;

[MappingTarget<OrderItem>(NestedTargetTypes = [typeof(OrderItemOptionDto)])]
public partial record OrderItemDto;

[MappingTarget<Order>(NestedTargetTypes = [typeof(OrderItemDto)])]
public partial record OrderDto;

// Usage
var order = new Order
{
    Items = new List<OrderItem>
    {
        new()
        {
            ProductName = "Laptop",
            Options = new List<OrderItemOption>
            {
                new() { Name = "Extended Warranty" },
                new() { Name = "Gift Wrap" }
            }
        }
    }
};

var dto = new OrderDto(order);
// dto.Items[0].Options is List<OrderItemOptionDto>
```

### Mixing Collections and Single Properties

You can have both collection and single nested target types in the same entity:

```csharp
public class Order
{
    public int Id { get; set; }
    public Address ShippingAddress { get; set; }      // Single nested object
    public Address BillingAddress { get; set; }       // Another single nested object
    public List<OrderItem> Items { get; set; }        // Collection of nested objects
}

[MappingTarget<Address>]
public partial record AddressDto;

[MappingTarget<OrderItem>]
public partial record OrderItemDto;

[MappingTarget<Order>(NestedTargetTypes = [typeof(AddressDto), typeof(OrderItemDto)])]
public partial record OrderDto;

// Generated OrderDto will have:
// - AddressDto ShippingAddress
// - AddressDto BillingAddress
// - List<OrderItemDto> Items
```

### EF Core Projections with Collection Nested Target Types

Collections work seamlessly with Entity Framework Core queries:

```csharp
// Efficient database projection
var orders = await dbContext.Orders
    .Include(o => o.Items)  // Include related data
    .Where(o => o.OrderDate >= DateTime.Today.AddDays(-30))
    .Select(OrderDto.Projection)
    .ToListAsync();

// The generated Projection property handles collections automatically:
// source => new OrderDto
// {
//     Id = source.Id,
//     OrderNumber = source.OrderNumber,
//     Items = source.Items.Select(x => new OrderItemDto(x)).ToList()
// }
```

### Empty Collections

Empty collections are handled gracefully:

```csharp
var order = new Order
{
    Id = 1,
    OrderNumber = "EMPTY",
    Items = new List<OrderItem>()  // Empty collection
};

var dto = new OrderDto(order);
// dto.Items is an empty List<OrderItemDto>, not null
```

### Collection Type Preservation

The original collection type is preserved during mapping:

```csharp
public class Team
{
    public Employee[] Members { get; set; }  // Array
}

[MappingTarget<Employee>]
public partial record EmployeeDto;

[MappingTarget<Team>(NestedTargetTypes = [typeof(EmployeeDto)])]
public partial record TeamDto;

// Generated TeamDto has:
// public EmployeeDto[] Members { get; init; }  // Stays as array

var team = new Team { Members = new[] { employee1, employee2 } };
var dto = new TeamDto(team);
// dto.Members is EmployeeDto[] (array type preserved)
```

### Real-World Example: E-Commerce Order System

```csharp
// Domain entities
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    public Product Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public Customer Customer { get; set; }
    public List<OrderItem> Items { get; set; }
    public decimal TotalAmount { get; set; }
}

// DTOs with nested target types
[MappingTarget<Product>]
public partial record ProductDto;

[MappingTarget<OrderItem>(NestedTargetTypes = [typeof(ProductDto)])]
public partial record OrderItemDto;

[MappingTarget<Customer>]
public partial record CustomerDto;

[MappingTarget<Order>(NestedTargetTypes = [typeof(CustomerDto), typeof(OrderItemDto)])]
public partial record OrderDto;

// Usage in API
[HttpGet("orders/{id}")]
public async Task<ActionResult<OrderDto>> GetOrder(int id)
{
    var order = await dbContext.Orders
        .Include(o => o.Customer)
        .Include(o => o.Items)
            .ThenInclude(i => i.Product)
        .FirstOrDefaultAsync(o => o.Id == id);

    if (order == null) return NotFound();

    // Automatic nested and collection mapping
    return new OrderDto(order);
}
```

### Collection Nested Target Types Best Practices

1. **Define target types in dependency order**: Define child target types before parent target types
   ```csharp
   [MappingTarget<OrderItem>]      // Define child first
   public partial record OrderItemDto;

   [MappingTarget<Order>(NestedTargetTypes = [typeof(OrderItemDto)])]  // Then parent
   public partial record OrderDto;
   ```

2. **Use collections for one-to-many relationships**: Perfect for Entity Framework navigation properties
   ```csharp
   public class Order
   {
       public List<OrderItem> Items { get; set; }  // One-to-many
   }
   ```

3. **Consider performance with large collections**: Be mindful when mapping large collections in memory
   ```csharp
   // For very large collections, consider pagination
   var recentOrders = dbContext.Orders
       .OrderByDescending(o => o.OrderDate)
       .Take(50)  // Limit collection size
       .Select(OrderDto.Projection)
       .ToListAsync();
   ```

4. **Handle null collections**: Initialize collections to avoid null reference exceptions
   ```csharp
   public class Order
   {
       public List<OrderItem> Items { get; set; } = new();  // Initialize
   }
   ```

### Benefits of Collection Nested Target Types

1. **Automatic Collection Mapping**: No manual LINQ Select calls needed
2. **Type Safety**: Compiler-verified collection element types
3. **Bidirectional Support**: Both forward and reverse (`ToSource()`) mapping
4. **EF Core Optimized**: Works efficiently with database projections
5. **Preserves Collection Types**: Lists stay lists, arrays stay arrays
6. **Multi-Level Support**: Unlimited nesting depth for collections

## Inheritance and Base Classes

### Including Properties from Base Classes

Include mode works seamlessly with inheritance:

```csharp
public class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
}

public class Product : BaseEntity
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
}

// Include properties from both base and derived class
[MappingTarget<Product>(Include = [nameof(Product.Id), nameof(Product.Name), nameof(Product.Price)])]
public partial class ProductSummaryDto;

// Include only derived class properties
[MappingTarget<Product>(Include = [nameof(Product.Name), nameof(Product.Category)])]
public partial class ProductInfoDto;
```

### Nested Classes

Both include and exclude work with nested classes:

```csharp
public class OuterClass
{
    [MappingTarget<User>(Include = [nameof(User.FirstName), nameof(User.LastName)])]
    public partial class NestedUserDto { }
}
```

## Custom Mapping with Include

You can combine Include mode with custom mapping:

```csharp
public class UserIncludeMapper : IMappingConfiguration<User, UserFormattedDto>
{
    public static void Map(User source, UserFormattedDto target)
    {
        target.DisplayName = $"{source.FirstName} {source.LastName}".ToUpper();
    }
}

[MappingTarget<User>(Include = [nameof(User.FirstName), nameof(User.LastName)], Configuration = typeof(UserIncludeMapper))]
public partial class UserFormattedDto
{
    public string DisplayName { get; set; } = string.Empty;
}
```

## Nullable Properties for Query and Patch Models

The `NullableProperties` parameter makes all non-nullable properties nullable in the generated target, which is extremely useful for query DTOs and partial update scenarios.

### Query/Filter DTOs

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
}

// All properties become nullable for flexible querying
[MappingTarget<Product>("CreatedAt", NullableProperties = true, GenerateToSource = false)]
public partial class ProductQueryDto;

// Usage: Only specify the fields you want to filter on
var query = new ProductQueryDto
{
    Name = "Widget",           // Filter by name
    Price = 50.00m,            // Filter by price
    IsAvailable = true         // Filter by availability
    // Id, CategoryId remain null (not part of filter)
};

// Use in LINQ queries
var results = products.Where(p =>
    (query.Name == null || p.Name.Contains(query.Name)) &&
    (query.Price == null || p.Price == query.Price) &&
    (query.IsAvailable == null || p.IsAvailable == query.IsAvailable)
).ToList();
```

### Patch/Update Models

```csharp
// Create a patch model where only non-null fields are updated
[MappingTarget<User>("Id", "CreatedAt", NullableProperties = true, GenerateToSource = false)]
public partial class UserPatchDto;

// Usage: Only update specific fields
var patch = new UserPatchDto
{
    Email = "newemail@example.com",  // Update email
    IsActive = false                 // Update active status
    // Other properties remain null (won't be updated)
};

// Apply the patch
void ApplyPatch(User user, UserPatchDto patch)
{
    if (patch.FirstName != null) user.FirstName = patch.FirstName;
    if (patch.LastName != null) user.LastName = patch.LastName;
    if (patch.Email != null) user.Email = patch.Email;
    if (patch.IsActive != null) user.IsActive = patch.IsActive.Value;
    // ... etc
}
```

### How NullableProperties Works

- **Value Types**: Become nullable (`int` → `int?`, `bool` → `bool?`, `DateTime` → `DateTime?`, enums → `EnumType?`)
- **Reference Types**: Remain reference types but are marked as nullable (`string` → `string`)
- **Already Nullable Types**: Stay nullable (`DateTime?` remains `DateTime?`)

### Important Considerations

1. **Disable GenerateToSource**: When using `NullableProperties = true`, set `GenerateToSource = false` since mapping nullable properties back to non-nullable source properties is not logically sound.

2. **Constructor Behavior**: The generated constructor will still map from source to nullable properties correctly.

3. **Comparison with GenerateDtos Query**: This provides the same functionality as the Query DTOs in `GenerateDtos`, but gives you more control with the `MappingTarget` attribute.

```csharp
// Similar to GenerateDtos Query DTO
[MappingTarget<Product>(NullableProperties = true, GenerateToSource = false)]
public partial record ProductQueryRecord;
```

## Enum Conversion

The `ConvertEnumsTo` property converts all enum properties in the source type to `string` or `int` in the generated target, which is useful for API responses, serialization, and database storage.

### Convert to String

```csharp
public enum OrderStatus { Draft, Submitted, Processing, Completed, Cancelled }

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
}

[MappingTarget<Order>(ConvertEnumsTo = typeof(string), GenerateToSource = true)]
public partial class OrderDto;

// Usage
var order = new Order { Id = 1, CustomerName = "Alice", Status = OrderStatus.Processing, Total = 99.99m };
var dto = new OrderDto(order);
dto.Status // "Processing" (string, not OrderStatus)

// Round-trip
var entity = dto.ToSource();
entity.Status // OrderStatus.Processing
```

### Convert to Int

```csharp
[MappingTarget<Order>(ConvertEnumsTo = typeof(int), GenerateToSource = true)]
public partial class OrderIntDto;

var dto = new OrderIntDto(order);
dto.Status // 2 (int value of OrderStatus.Processing)
```

### Nullable Enum Handling

Nullable enum properties preserve their nullability after conversion:

```csharp
public class Entity
{
    public int Id { get; set; }
    public OrderStatus? Status { get; set; }       // Nullable
    public OrderStatus Priority { get; set; }      // Non-nullable
}

[MappingTarget<Entity>(ConvertEnumsTo = typeof(string))]
public partial class EntityStringDto;
// Status: string (null when source is null)
// Priority: string

[MappingTarget<Entity>(ConvertEnumsTo = typeof(int))]
public partial class EntityIntDto;
// Status: int? (nullable)
// Priority: int
```

### Combining with NullableProperties

```csharp
[MappingTarget<Order>(ConvertEnumsTo = typeof(string), NullableProperties = true, GenerateToSource = false)]
public partial class OrderQueryDto;
// All properties nullable + enums as strings - perfect for filter DTOs
```

### EF Core Projections

Enum conversions are included in the generated Projection expression and translate correctly to SQL:

```csharp
var results = await dbContext.Orders
    .Where(o => o.Status == OrderStatus.Completed)
    .Select(OrderDto.Projection)
    .ToListAsync();
// Status column returned as string in the DTO
```

### Important Notes

- **All enums are converted**: The setting applies to every enum property. For mixed behavior, use separate target DTOs or custom configurations.
- **Non-enum properties are unaffected**: Only enum-typed properties are converted.
- **Supported target types**: Only `typeof(string)` and `typeof(int)` are supported.

See [Enum Conversion](20_ConvertEnumsTo.md) for the full reference.

## Mixed Usage Patterns

### API Layer Pattern

```csharp
// Controller uses different target DTOs for different endpoints
[ApiController]
public class UsersController : ControllerBase
{
    [HttpGet]
    public List<UserSummaryDto> GetUsers()
    {
        return users.SelectTargets<User, UserSummaryDto>().ToList();
    }
    
    [HttpGet("{id}")]
    public UserDetailDto GetUser(int id)
    {
        return user.ToTarget<User, UserDetailDto>();
    }
    
    [HttpPost]
    public IActionResult CreateUser(UserCreateDto dto)
    {
        var user = dto.ToSource();
        // Save user...
    }
}

// Different DTOs for different use cases
[MappingTarget<User>(Include = [nameof(User.Id), nameof(User.FirstName), nameof(User.LastName)])]
public partial record UserSummaryDto;

[MappingTarget<User>(nameof(User.Password))] // Exclude password but include everything else
public partial class UserDetailDto;

[MappingTarget<User>(Include = [nameof(User.FirstName), nameof(User.LastName), nameof(User.Email), nameof(User.Department)])]
public partial class UserCreateDto;
```

## Record Types with Include

Include mode works perfectly with modern C# records:

```csharp
public record ModernUser
{
    public required string Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string? Bio { get; set; }
}

// Generate record with only specific properties
[MappingTarget<ModernUser>(Include = [nameof(ModernUser.FirstName), nameof(ModernUser.LastName), nameof(ModernUser.Email)])]
public partial record ModernUserContactRecord;

// Include with init-only preservation
[MappingTarget<ModernUser>(Include = [nameof(ModernUser.Id), nameof(ModernUser.FirstName), nameof(ModernUser.LastName)],
       PreserveInitOnlyProperties = true)]
public partial record ModernUserImmutableRecord;
```

## Performance Considerations

### Include vs Exclude Performance

- **Include mode**: Generates smaller target DTOs, which can improve serialization performance and reduce memory usage
- **Exclude mode**: Better when you need most properties from the source type

### Generated Code Comparison

```csharp
// Include mode - generates minimal code
[MappingTarget<User>(Include = [nameof(User.FirstName), nameof(User.Email)])]
public partial class UserMinimalDto;
// Generated: only FirstName and Email properties

// Exclude mode - generates more code
[MappingTarget<User>(nameof(User.Password))]
public partial class UserFullDto;
// Generated: all properties except Password
```

## ToSource Method Behavior with Include

When using Include mode, the `ToSource()` method generates source objects with default values for non-included properties:

```csharp
[MappingTarget<User>(Include = [nameof(User.FirstName), nameof(User.LastName), nameof(User.Email)])]
public partial class UserContactDto;

var dto = new UserContactDto();
var sourceUser = dto.ToSource();

// sourceUser.FirstName = dto.FirstName (copied)
// sourceUser.LastName = dto.LastName (copied)
// sourceUser.Email = dto.Email (copied)
// sourceUser.Id = 0 (default for int)
// sourceUser.Password = string.Empty (default for string)
// sourceUser.IsActive = false (default for bool)
```

## Attribute Copying for Validation and Metadata

The `CopyAttributes` parameter enables automatic copying of attributes from source type members to the generated target. This is particularly useful for preserving validation attributes, display metadata, and JSON serialization settings in DTOs.

### Basic Attribute Copying

```csharp
public class CreateUserRequest
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be 2-50 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    public int Age { get; set; }

    [Phone(ErrorMessage = "Invalid phone number")]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? Bio { get; set; }
}

// Generate DTO with all validation attributes copied
[MappingTarget<CreateUserRequest>(CopyAttributes = true)]
public partial class CreateUserDto;
```

The generated `CreateUserDto` will include all the validation attributes:

```csharp
public partial class CreateUserDto
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be 2-50 characters")]
    public string FirstName { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; }

    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    public int Age { get; set; }

    [Phone(ErrorMessage = "Invalid phone number")]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? Bio { get; set; }
}
```

### Combining with Exclude/Include

Attributes are only copied for properties that are included in the target DTO:

```csharp
public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}

// Exclude password - its attributes won't be copied
[MappingTarget<User>("Password", CopyAttributes = true)]
public partial class UserDto;
// UserDto has Required, StringLength, EmailAddress on included properties
// No attributes from Password property

// Include only specific properties - only those get attributes
[MappingTarget<User>(Include = [nameof(User.FirstName), nameof(User.Email)], CopyAttributes = true)]
public partial class UserContactDto;
// UserContactDto only has attributes for FirstName and Email
```

### With Nested Target Types

Attribute copying works seamlessly with nested target types:

```csharp
public class Address
{
    [Required]
    [StringLength(100)]
    public string Street { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string City { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Invalid ZIP code")]
    public string ZipCode { get; set; } = string.Empty;
}

public class Order
{
    [Required]
    [StringLength(20)]
    public string OrderNumber { get; set; } = string.Empty;

    [Range(0.01, 1000000)]
    public decimal TotalAmount { get; set; }

    public Address ShippingAddress { get; set; } = null!;

    public string? InternalNotes { get; set; }
}

// Both target types copy their respective attributes
[MappingTarget<Address>(CopyAttributes = true)]
public partial class AddressDto;

[MappingTarget<Order>("InternalNotes", CopyAttributes = true, NestedTargetTypes = [typeof(AddressDto)])]
public partial class OrderDto;

// Usage - validation works on both parent and nested objects
var orderDto = new OrderDto
{
    OrderNumber = "ORD-12345",
    TotalAmount = 99.99m,
    ShippingAddress = new AddressDto
    {
        Street = "123 Main St",
        City = "Springfield",
        ZipCode = "12345"
    }
};

// ASP.NET Core will validate all attributes including nested ones
var validationContext = new ValidationContext(orderDto);
var validationResults = new List<ValidationResult>();
bool isValid = Validator.TryValidateObject(orderDto, validationContext, validationResults, validateAllProperties: true);
```

### Display and JSON Attributes

Attribute copying works with more than just validation attributes:

```csharp
public class Product
{
    [Display(Name = "Product Name", Description = "The name of the product")]
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Unit Price")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
    [Range(0.01, 10000)]
    public decimal Price { get; set; }

    [JsonPropertyName("product_sku")]
    [Required]
    public string Sku { get; set; } = string.Empty;

    [Display(Name = "Available")]
    public bool IsAvailable { get; set; }
}

[MappingTarget<Product>(CopyAttributes = true)]
public partial class ProductDto;

// ProductDto has all Display, DisplayFormat, JsonPropertyName, Required, StringLength, and Range attributes
```

### Smart Filtering

The attribute copier automatically excludes:

1. **Compiler-generated attributes** (e.g., `System.Runtime.CompilerServices.*`)
2. **The base ValidationAttribute class** (only derived validation attributes are copied)
3. **Attributes not valid for the target member** based on `AttributeUsage`

```csharp
// Example: These attributes would NOT be copied
[CompilerGenerated]  // Excluded: compiler-generated
[ValidationAttribute]  // Excluded: base class itself
[Table("Users")]  // Excluded if AttributeUsage doesn't allow properties
public class User { ... }
```

### API Validation Pattern

A common use case is ensuring DTOs have the same validation as domain models:

```csharp
// Domain model with validation
public class RegisterUserCommand
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$", ErrorMessage = "Password must contain uppercase, lowercase, and number")]
    public string Password { get; set; } = string.Empty;
}

// API request DTO with copied validation
[MappingTarget<RegisterUserCommand>(CopyAttributes = true)]
public partial class RegisterUserRequest;

// API controller automatically validates using copied attributes
[ApiController]
public class AuthController : ControllerBase
{
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterUserRequest request)
    {
        // ModelState.IsValid uses the copied validation attributes
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var command = new RegisterUserCommand
        {
            Username = request.Username,
            Email = request.Email,
            Password = request.Password
        };

        // Process registration...
        return Ok();
    }
}
```

### Customizing After Copy

You can add additional attributes to the partial class:

```csharp
public class User
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

[MappingTarget<User>(CopyAttributes = true)]
public partial class UserDto
{
    // Add custom property with its own attributes
    [Required]
    [MinLength(10)]
    public string CustomField { get; set; } = string.Empty;
}

// Generated properties will have copied attributes
// Custom properties keep their own attributes
```

### When to Use CopyAttributes

**Use `CopyAttributes = true` when:**
- Creating API request/response DTOs that need the same validation as domain models
- Building form models for UI frameworks
- Ensuring consistency between entity and DTO validation
- Preserving display metadata for data grids and forms
- Maintaining JSON serialization settings across layers

**Don't use it when:**
- DTOs need different validation rules than the source
- You want to avoid tight coupling between domain and API models
- Source types have ORM or infrastructure-specific attributes

## Best Practices

### When to Use Include

1. **API Responses**: Create focused DTOs for API endpoints
2. **Search Results**: Include only essential data for search listings
3. **Mobile Apps**: Minimize data transfer with targeted DTOs
4. **Microservices**: Create service-specific views of shared models

### When to Use Exclude

1. **Security**: Hide sensitive fields while keeping everything else
2. **Legacy Code**: Maintain existing patterns and behavior
3. **Large Models**: When you need most properties from complex entities

### Naming Conventions

```csharp
// Descriptive names for include-based DTOs
[MappingTarget<User>(Include = [nameof(User.FirstName), nameof(User.LastName)])]
public partial class UserNameOnlyDto; // Clear about what's included

[MappingTarget<Product>(Include = [nameof(Product.Id), nameof(Product.Name), nameof(Product.Price)])]
public partial class ProductListItemDto; // Indicates usage context

// Traditional names for exclude-based DTOs  
[MappingTarget<User>(nameof(User.Password))]
public partial class UserDto; // General DTO name when excluding few fields
```

---

See [Expression Mapping](10_ExpressionMapping.md) for advanced query scenarios and [Custom Mapping](04_CustomMapping.md) for complex transformation logic.
