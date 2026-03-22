# Extension Methods (LINQ, EF Core, etc.)

DotNetBrightener.Mapper.Mapping provides a set of provider-agnostic extension methods for mapping and projecting between your domain entities and generated target types.
For async EF Core support, see the separate DotNetBrightener.Mapper.Mapping.EFCore package.

## Methods (DotNetBrightener.Mapper.Mapping)

### Mappings

| Method                              | Description                                                      |
|------------------------------------- |------------------------------------------------------------------|
| `ToTarget<TSource, TTarget>()`        | Map a single object with explicit source type (compile-time).   |
| `ToTarget<TTarget>()`                 | Map a single object with inferred source type (runtime).        |
| `ToSource<TTarget, TSource>()`   | Map target back to source via generated ToSource method.         |
| `ToSource<TSource>()`           | Map target back to source with inferred target type.              |
| `SelectTargets<TSource, TTarget>()`   | Map an `IEnumerable<TSource>` with explicit types.              |
| `SelectTargets<TTarget>()`            | Map an `IEnumerable` with inferred source type.                 |
| `SelectSources<TTarget, TSource>()` | Map targets back to sources.                             |
| `SelectSources<TSource>()` | Map targets back to sources with inferred target type.            |
| `SelectTarget<TSource, TTarget>()`    | Project an `IQueryable<TSource>` with explicit types.           |
| `SelectTarget<TTarget>()`             | Project an `IQueryable` with inferred source type.              |

### Patch/Update methods (Target -> Source)

| Method                                      | Description                                                      |
|---------------------------------------------|------------------------------------------------------------------|
| `ApplyTarget<TSource, TTarget>()`             | Apply changed properties from target to source  |
| `ApplyTarget<TTarget>()`                      | Apply changed properties with inferred source type.              |
| `ApplyTargetWithChanges<TSource, TTarget>()`  | Apply changes and return `TargetApplyResult` with changed property names. |

## Methods (DotNetBrightener.Mapper.Mapping.EFCore)

| Method                              | Description                                                      |
|------------------------------------- |------------------------------------------------------------------|
| `ToTargetsAsync<TSource, TTarget>()`  | Async projection to `List<TTarget>` with explicit source type.    |
| `ToTargetsAsync<TTarget>()`           | Async projection to `List<TTarget>` with inferred source type.    |
| `FirstAsync<TSource, TTarget>()`| Async projection to first/default with explicit source type.      |
| `FirstAsync<TTarget>()`         | Async projection to first/default with inferred source type.      |
| `SingleAsync<TSource, TTarget>()`| Async projection to single with explicit source type.            |
| `SingleAsync<TTarget>()`        | Async projection to single with inferred source type.            |
| `UpdateFromTarget<TEntity, TTarget>()` | Update entity with changed properties from target DTO.            |
| `UpdateFromTargetAsync<TEntity, TTarget>()`| Async update entity with changed properties from target DTO.  |
| `UpdateFromTargetWithChanges<TEntity, TTarget>()`| Update entity and return information about changed properties. |

## Methods (DotNetBrightener.Mapper.Mapping.EFCore)

For advanced custom async mapper support, use the overloads provided by the same EF Core package:

```bash
dotnet add package DotNetBrightener.Mapper.Mapping.EFCore
```

| Method                              | Description                                                      |
|------------------------------------- |------------------------------------------------------------------|
| `ToTargetsAsync<TSource, TTarget>(mapper)` | Async projection with custom instance mapper (DI support).    |
| `ToTargetsAsync<TSource, TTarget, TAsyncMapper>()` | Async projection with static async mapper.              |
| `FirstAsync<TSource, TTarget>(mapper)` | Get first with custom instance mapper (DI support).        |
| `FirstAsync<TSource, TTarget, TAsyncMapper>()` | Get first with static async mapper.                   |
| `SingleAsync<TSource, TTarget>(mapper)` | Get single with custom instance mapper (DI support).      |
| `SingleAsync<TSource, TTarget, TAsyncMapper>()` | Get single with static async mapper.                 |

## Usage Examples

### Extensions

```bash
dotnet add package DotNetBrightener.Mapper.Mapping
```

```csharp
using DotNetBrightener.Mapper.Mapping;

// Forward mapping: Source -> Target
var dto = person.ToTarget<PersonDto>();

// Enumerable mapping
var dtos = people.SelectTargets<PersonDto>();

// Reverse mapping: Target -> Source (apply changes)
var updateDto = new PersonDto { Name = "Jane", Email = "jane@example.com" };
person.ApplyTarget(updateDto);  // Only updates changed properties

// Track changes for auditing
var result = person.ApplyTargetWithChanges<Person, PersonDto>(updateDto);

if (result.HasChanges)
{
    Console.WriteLine($"Changed: {string.Join(", ", result.ChangedProperties)}");
}
```

### EF Core Extensions

```bash
dotnet add package DotNetBrightener.Mapper.Mapping.EFCore
```

```csharp
// IQueryable (LINQ/EF Core)

using DotNetBrightener.Mapper.Mapping.EFCore;

var query = dbContext.People.SelectTarget<PersonDto>();

// Async (EF Core)
var dtosAsync = await dbContext.People.ToTargetsAsync<PersonDto>();
var dtosInferred = await dbContext.People.ToTargetsAsync<PersonDto>();

var firstDto = await dbContext.People.FirstAsync<Person, PersonDto>();
var firstInferred = await dbContext.People.FirstAsync<PersonDto>();

var singleDto = await dbContext.People.SingleAsync<Person, PersonDto>();
var singleInferred = await dbContext.People.SingleAsync<PersonDto>();
```

#### Automatic Navigation Property Loading (No `.Include()` Required!)

When using nested target types, EF Core automatically loads navigation properties without requiring explicit `.Include()` calls:

```csharp
// Define nested target types
[MappingTarget<Address>]
public partial record AddressDto;

[MappingTarget<Company>(NestedTargetTypes = [typeof(AddressDto)])]
public partial record CompanyDto;

// Navigation properties are automatically loaded!
var companies = await dbContext.Companies
    .Where(c => c.IsActive)
    .ToTargetsAsync<CompanyDto>();

// The HeadquartersAddress navigation property is automatically included
// EF Core sees the property access in the projection and generates JOINs

// Works with all projection methods:
await dbContext.Companies.ToTargetsAsync<CompanyDto>();       
await dbContext.Companies.FirstAsync<CompanyDto>();    
await dbContext.Companies.SelectTarget<CompanyDto>().ToListAsync();

// Also works with collecstions:
[MappingTarget<OrderItem>]
public partial record OrderItemDto;

[MappingTarget<Order>(NestedTargetTypes = [typeof(OrderItemDto), typeof(AddressDto)])]
public partial record OrderDto;

var orders = await dbContext.Orders.ToTargetsAsync<OrderDto>();
// Automatically includes Items collection and ShippingAddress!
```

### Reverse Mapping: ApplyTarget

For general-purpose patch/update scenarios

```csharp
using DotNetBrightener.Mapper.Mapping;

[HttpPut("{id}")]
public IActionResult UpdatePerson(int id, [FromBody] PersonDto dto)
{
    var person = repository.GetById(id);
    if (person == null) return NotFound();

    // Apply changes from target to source (no DbContext required)
    var result = person.ApplyTargetWithChanges<Person, PersonDto>(dto);

    if (result.HasChanges)
    {
        repository.Save(person);
        logger.LogInformation("Person {Id} updated: {Changes}",
            id, string.Join(", ", result.ChangedProperties));
    }

    return NoContent();
}
```

### Reverse Mapping: UpdateFromTarget (EF Core)

For EF Core-specific scenarios with change tracking integration:

```csharp
using DotNetBrightener.Mapper.Mapping.EFCore;

[HttpPut("{id}")]
public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
{
    var user = await context.Users.FindAsync(id);
    if (user == null) return NotFound();

    // Only updates properties that actually changed - selective update
    // Integrates with EF Core's change tracking
    user.UpdateFromTarget(dto, context);

    await context.SaveChangesAsync();
    return Ok();
}

// With change tracking for auditing
var result = user.UpdateFromTargetWithChanges(dto, context);
if (result.HasChanges)
{
    logger.LogInformation("User {UserId} updated. Changed: {Properties}",
        user.Id, string.Join(", ", result.ChangedProperties));
}

// Async version
await user.UpdateFromTargetAsync(dto, context);
```

**Key Differences:**
- **`ApplyTarget`** (DotNetBrightener.Mapper.Mapping): No EF Core dependency, uses reflection, works with any objects
- **`UpdateFromTarget`** (DotNetBrightener.Mapper.Mapping.EFCore): Requires `DbContext`, integrates with EF Core change tracking

### Complete API Example

```csharp
// Domain model
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }  // Sensitive
    public DateTime CreatedAt { get; set; }  // Immutable
}

// Update DTO - excludes sensitive/immutable properties
[MappingTarget<User>("Password", "CreatedAt")]
public partial class UpdateUserDto { }

// API Controller
[ApiController]
public class UsersController : ControllerBase
{
    // GET: Entity -> Target
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null) return NotFound();

        return user.ToTarget<UserDto>();  // Forward mapping
    }

    // PUT: Target -> Entity
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.UpdateFromTarget(dto, context);  // Reverse mapping
        await context.SaveChangesAsync();

        return NoContent();
    }
}

// Non-EF Core version with ApplyTarget
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repository;

    // PUT: Target -> Entity (selective update without EF Core)
    [HttpPut("{id}")]
    public IActionResult UpdateUser(int id, UpdateUserDto dto)
    {
        var user = _repository.GetById(id);
        if (user == null) return NotFound();

        user.ApplyTarget(dto);  // Reverse mapping (no DbContext)
        _repository.Save(user);

        return NoContent();
    }
}
```

### EF Core Custom Mappers (Advanced)

For complex mappings that cannot be expressed as SQL projections (e.g., calling external services, complex type conversions like Vector2, or async operations), install the advanced mapping package:

```bash
dotnet add package DotNetBrightener.Mapper.Mapping.EFCore
```

```csharp
using DotNetBrightener.Mapper.Mapping.EFCore;  // Advanced mappers
using DotNetBrightener.Mapper.Mapping;

// Define your DTO with excluded properties
[MappingTarget<User>("X", "Y")]
public partial class UserDto
{
    public Vector2 Position { get; set; }
}

// Option 1: Static mapper (no DI)
public class UserMapper : IMappingConfigurationAsync<User, UserDto>
{
    public static async Task MapAsync(User source, UserDto target, CancellationToken cancellationToken = default)
    {
        target.Position = new Vector2(source.X, source.Y);
    }
}

// Option 2: Instance mapper with dependency injection
public class UserMapper : IMappingConfigurationAsyncInstance<User, UserDto>
{
    private readonly ILocationService _locationService;

    public UserMapper(ILocationService locationService)
    {
        _locationService = locationService;
    }

    public async Task MapAsync(User source, UserDto target, CancellationToken cancellationToken = default)
    {
        target.Position = new Vector2(source.X, source.Y);
        target.Location = await _locationService.GetLocationAsync(source.LocationId);
    }
}

// Usage with static mapper
var users = await dbContext.Users
    .Where(u => u.IsActive)
    .ToTargetsAsync<User, UserDto, UserMapper>();

// Usage with instance mapper (DI)
var users = await dbContext.Users
    .Where(u => u.IsActive)
    .ToTargetsAsync<User, UserDto>(userMapper);
```

**Note:** Custom mapper methods materialize the query first (execute SQL), then apply your custom logic. All matching properties are auto-mapped first.

See the [DotNetBrightener.Mapper.Mapping.EFCore](https://www.nuget.org/packages/DotNetBrightener.Mapper.Mapping.EFCore) package for more details.

---

See [Quick Start](02_QuickStart.md) for setup and [DotNetBrightener.Mapper.Mapping.EFCore](https://www.nuget.org/packages/DotNetBrightener.Mapper.Mapping.EFCore) for EF Core projection and update helpers.
