# Property Mapping with MapFrom

The `[MapFrom]` attribute provides declarative property mapping, allowing you to rename properties without implementing a full custom mapping configuration. This is perfect for simple property renames, API response shaping, and maintaining clean separation between domain and DTO property names.

## Basic Usage

Use `[MapFrom]` on properties in your target type to specify which source property to map from. **Always use `nameof()` for type-safe property references**:

```csharp
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
}

[MappingTarget<User>(GenerateToSource = true)]
public partial class UserDto
{
    [MapFrom(nameof(User.FirstName), Reversible = true)]
    public string Name { get; set; } = string.Empty;

    [MapFrom(nameof(User.LastName), Reversible = true)]
    public string FamilyName { get; set; } = string.Empty;
}
```

This generates:
- **Constructor**: Maps `source.FirstName` to `Name` and `source.LastName` to `FamilyName`
- **Projection**: Uses the same mapping for EF Core queries
- **ToSource()**: Reverses the mapping automatically

## How It Works

When you use `[MapFrom]`:

1. **The source property is not auto-generated** - You declare the target property with its new name
2. **Mapping is automatic** - Constructor, Projection, and ToSource all use the mapping
3. **Other properties remain unchanged** - Properties without `[MapFrom]` work normally

```csharp
var user = new User
{
    Id = 1,
    FirstName = "John",
    LastName = "Doe",
    Email = "john@example.com",
    Age = 30
};

var dto = new UserDto(user);
// dto.Id = 1 (auto-mapped)
// dto.Name = "John" (mapped from FirstName)
// dto.FamilyName = "Doe" (mapped from LastName)
// dto.Email = "john@example.com" (auto-mapped)
// dto.Age = 30 (auto-mapped)

// Reverse mapping
var entity = dto.ToSource();
// entity.FirstName = "John" (mapped from Name)
// entity.LastName = "Doe" (mapped from FamilyName)
```

## Attribute Properties

### Source (Required)

The source property path or expression to map from. **Always use `nameof()` for type-safe references**:

```csharp
// Type-safe property reference (recommended)
[MapFrom(nameof(User.FirstName))]
public string Name { get; set; }

// Nested property path (use @ prefix for full path validation)
[MapFrom(nameof(@User.Company.Name))]
public string CompanyName { get; set; }

// Expression with multiple properties using nameof() concatenation
[MapFrom(nameof(User.FirstName) + " + \" \" + " + nameof(User.LastName))]
public string FullName { get; set; }
```

### Reversible

Controls whether the mapping is included in `ToSource()`. Default is `false` (opt-in).

```csharp
// This property WILL be mapped back to the source
[MapFrom(nameof(User.FirstName), Reversible = true)]
public string Name { get; set; } = string.Empty;

// This property will NOT be mapped back (default)
[MapFrom(nameof(User.LastName))]
public string DisplayName { get; set; } = string.Empty;
```

Use `Reversible = true` when:
- You need the mapping to work both ways (source ↔ DTO)
- The property should be included in `ToSource()` output

Keep `Reversible = false` (default) for:
- One-way mappings (source → DTO only)
- Read-only DTOs that don't need reverse mapping
- Properties that shouldn't modify the source entity

### IncludeInProjection

Controls whether the mapping is included in the static `Projection` expression. Default is `true`.

```csharp
// This property will NOT be included in EF Core projections
[MapFrom(nameof(Product.ComplexField), IncludeInProjection = false)]
public string Computed { get; set; } = string.Empty;
```

Use `IncludeInProjection = false` for:
- Mappings that cannot be translated to SQL
- Properties requiring client-side evaluation
- Complex expressions that EF Core doesn't support

## Examples

### Simple Property Rename

```csharp
[MappingTarget<Customer>(GenerateToSource = true)]
public partial class CustomerDto
{
    [MapFrom(nameof(Customer.CompanyName), Reversible = true)]
    public string Company { get; set; } = string.Empty;

    [MapFrom(nameof(Customer.ContactName), Reversible = true)]
    public string Contact { get; set; } = string.Empty;
}
```

### One-Way Mapping (Default)

```csharp
[MappingTarget<Product>]
public partial class ProductDto
{
    // Display-only property, default is not reversible
    [MapFrom(nameof(Product.Name))]
    public string ProductTitle { get; set; } = string.Empty;
}
```

### Computed Expressions

Use `nameof()` concatenation to create expressions that combine or transform properties:

```csharp
[MappingTarget<User>]
public partial class UserDto
{
    // Concatenate first and last name using nameof()
    [MapFrom(nameof(User.FirstName) + " + \" \" + " + nameof(User.LastName))]
    public string FullName { get; set; } = string.Empty;

    // Mathematical expressions
    [MapFrom(nameof(Order.Price) + " * " + nameof(Order.Quantity))]
    public decimal Total { get; set; }

    // Method calls (works in constructor, may not translate to SQL)
    [MapFrom(nameof(Product.Name) + ".ToUpper()")]
    public string UpperName { get; set; } = string.Empty;
}
```

**Note:** Complex expressions may not translate to SQL in EF Core projections. Use `IncludeInProjection = false` for expressions that require client-side evaluation.

### With Nested Target Types

`MapFrom` works with nested target types too:

```csharp
[MappingTarget<Company>(GenerateToSource = true)]
public partial class CompanyDto
{
    [MapFrom(nameof(Company.CompanyName), Reversible = true)]
    public string Name { get; set; } = string.Empty;
}

[MappingTarget<Employee>(NestedTargetTypes = [typeof(CompanyDto)],
    GenerateToSource = true)]
public partial class EmployeeDto;
```

### Combining with Custom Configuration

MapFrom mappings are applied first, then your custom mapper runs:

```csharp
public class UserMapper : IMappingConfiguration<User, UserDto>
{
    public static void Map(User source, UserDto target)
    {
        // This runs AFTER MapFrom mappings are applied
        target.FullName = $"{target.Name} {target.FamilyName}";
    }
}

[MappingTarget<User>(Configuration = typeof(UserMapper), GenerateToSource = true)]
public partial class UserDto
{
    [MapFrom(nameof(User.FirstName), Reversible = true)]
    public string Name { get; set; } = string.Empty;

    [MapFrom(nameof(User.LastName), Reversible = true)]
    public string FamilyName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
}
```

## When to Use MapFrom vs Custom Configuration

| Scenario | MapFrom | Custom Config |
|----------|---------|---------------|
| Simple property rename | ✅ Best choice | Overkill |
| Multiple renames | ✅ Best choice | Overkill |
| Computed values (e.g., concatenation) | ✅ Supported | Alternative |
| Mathematical expressions | ✅ Supported | Alternative |
| Async operations | ❌ | ✅ Required |
| Complex transformations | ❌ | ✅ Required |
| Type conversions | ❌ | ✅ Required |
| Conditional logic | ❌ | ✅ Required |

## Best Practices

1. **Always use `nameof()` for type safety** - Prevents typos and enables refactoring support
2. **Use `@` prefix for nested paths** - `nameof(@SourceType.Nested.Property)` provides full path validation
3. **Set Reversible = false for computed properties** - Expressions can't be reversed
4. **Consider projection compatibility** - Set `IncludeInProjection = false` for expressions that can't translate to SQL
5. **Combine with custom mappers when needed** - MapFrom handles the basics, custom mapper handles the rest

## Nested Property Paths

Use `@nameof()` syntax to flatten nested object structures with full compile-time validation:

```csharp
public class Order
{
    public int Id { get; set; }
    public Customer? Customer { get; set; }
}

public class Customer
{
    public string Name { get; set; }
    public Address? Address { get; set; }
}

public class Address
{
    public string City { get; set; }
    public string Country { get; set; }
}

[MappingTarget<Order>(
    exclude: [nameof(Order.Customer)],
    GenerateToSource = false)]
public partial class OrderDto
{
    // Single-level nested path using @nameof()
    [MapFrom(nameof(@Order.Customer.Name))]
    public string CustomerName { get; set; } = string.Empty;

    // Multi-level nested path
    [MapFrom(nameof(@Order.Customer.Address.City))]
    public string City { get; set; } = string.Empty;

    [MapFrom(nameof(@Order.Customer.Address.Country))]
    public string Country { get; set; } = string.Empty;
}
```

**Important**: Nested property paths do not include null-checking. If any intermediate property is null, a `NullReferenceException` will be thrown. Ensure intermediate properties are non-nullable or handle null cases in your application logic.

## Limitations

- **Same type required** - Source and target property types must match
- **Expressions are not reversible** - Computed expressions are one-way (source → DTO only)
- **No null-checking in nested paths** - Accessing nested properties will throw if any intermediate property is null

For complex scenarios like async operations or conditional logic, use [Custom Mapping](04_CustomMapping.md) instead.
