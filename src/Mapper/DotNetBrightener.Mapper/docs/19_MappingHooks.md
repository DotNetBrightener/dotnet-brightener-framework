# Before/After Mapping Hooks

`DotNetBrightener.Mapper.Mapping` provides hooks to run custom logic before and after the automatic property mapping. This is useful for validation, setting defaults, computing derived values, or applying transformations.

## Overview

| Interface | When Called | Use Case |
|-----------|-------------|----------|
| `IBeforeMapConfiguration<TSource, TTarget>` | Before properties are copied | Validation, defaults, state preparation |
| `IAfterMapConfiguration<TSource, TTarget>` | After properties are copied | Computed values, transformations, post-processing |
| `IMapHooksConfiguration<TSource, TTarget>` | Both before and after | Combined scenarios |

## When to Use Mapping Hooks

### Use BeforeMap When:
- Validating input before mapping starts
- Setting default/timestamp values on the target
- Preparing state that affects the mapping
- Throwing validation errors before mapping completes

### Use AfterMap When:
- Computing derived values from mapped properties
- Transforming values after they're copied
- Applying business rules to the result
- Validating the final mapped result

### Use Configuration (Map) When:
- You only need to add computed properties
- Simple transformations that don't need "before" logic
- Legacy code or simpler scenarios

## Static Hooks (No Dependency Injection)

### BeforeMap Configuration

```csharp
using DotNetBrightener.Mapper.Mapping;

public class UserBeforeMapConfig : IBeforeMapConfiguration<User, UserDto>
{
    public static void BeforeMap(User source, UserDto target)
    {
        // Validate source
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        
        if (string.IsNullOrEmpty(source.Email))
            throw new ValidationException("Email is required");
        
        // Set default values on target
        target.MappedAt = DateTime.UtcNow;
        target.MappingVersion = "1.0";
    }
}

[MappingTarget<User>(BeforeMapConfiguration = typeof(UserBeforeMapConfig))]
public partial class UserDto
{
    public DateTime MappedAt { get; set; }
    public string MappingVersion { get; set; } = string.Empty;
}
```

### AfterMap Configuration

```csharp
using DotNetBrightener.Mapper.Mapping;

public class UserAfterMapConfig : IAfterMapConfiguration<User, UserDto>
{
    public static void AfterMap(User source, UserDto target)
    {
        // Compute derived values after properties are mapped
        target.FullName = $"{target.FirstName} {target.LastName}";
        target.Age = CalculateAge(source.DateOfBirth);
        target.IsAdult = target.Age >= 18;
        
        // Apply transformations
        target.Email = target.Email?.ToLowerInvariant();
    }
    
    private static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }
}

[MappingTarget<User>(AfterMapConfiguration = typeof(UserAfterMapConfig))]
public partial class UserDto
{
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool IsAdult { get; set; }
}
```

### Combined Hooks

Use `IMapHooksConfiguration` for both before and after logic in one class:

```csharp
using DotNetBrightener.Mapper.Mapping;

public class UserMappingHooks : IMapHooksConfiguration<User, UserDto>
{
    public static void BeforeMap(User source, UserDto target)
    {
        target.MappedAt = DateTime.UtcNow;
    }
    
    public static void AfterMap(User source, UserDto target)
    {
        target.FullName = $"{target.FirstName} {target.LastName}";
    }
}

// Reference the same class for both hooks
[MappingTarget<User>(BeforeMapConfiguration = typeof(UserMappingHooks),
    AfterMapConfiguration = typeof(UserMappingHooks))]
public partial class UserDto
{
    public DateTime MappedAt { get; set; }
    public string FullName { get; set; } = string.Empty;
}
```

## Instance Hooks (With Dependency Injection)

For scenarios requiring injected services:

### BeforeMap with DI

```csharp
using DotNetBrightener.Mapper.Mapping;

public class UserValidationHook : IBeforeMapConfigurationInstance<User, UserDto>
{
    private readonly IUserValidator _validator;
    
    public UserValidationHook(IUserValidator validator)
    {
        _validator = validator;
    }
    
    public void BeforeMap(User source, UserDto target)
    {
        var result = _validator.Validate(source);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
        
        target.MappedAt = DateTime.UtcNow;
    }
}
```

### AfterMap with DI

```csharp
using DotNetBrightener.Mapper.Mapping;

public class UserEnrichmentHook : IAfterMapConfigurationInstance<User, UserDto>
{
    private readonly IProfileService _profileService;
    private readonly IAgeCalculator _ageCalculator;
    
    public UserEnrichmentHook(IProfileService profileService, IAgeCalculator ageCalculator)
    {
        _profileService = profileService;
        _ageCalculator = ageCalculator;
    }
    
    public void AfterMap(User source, UserDto target)
    {
        target.FullName = $"{target.FirstName} {target.LastName}";
        target.Age = _ageCalculator.Calculate(source.DateOfBirth);
        target.ProfileUrl = _profileService.GetProfileUrl(source.Id);
    }
}
```

## Async Hooks

For operations requiring async I/O:

### Async BeforeMap

```csharp
using DotNetBrightener.Mapper.Mapping;

public class UserAsyncBeforeHook : IBeforeMapConfigurationAsync<User, UserDto>
{
    public static async Task BeforeMapAsync(User source, UserDto target, CancellationToken ct = default)
    {
        // Async validation
        var isValid = await ValidateUserAsync(source, ct);
        if (!isValid)
        {
            throw new ValidationException("User validation failed");
        }
        
        target.MappedAt = DateTime.UtcNow;
    }
}
```

### Async AfterMap

```csharp
using DotNetBrightener.Mapper.Mapping;

public class UserAsyncAfterHook : IAfterMapConfigurationAsync<User, UserDto>
{
    public static async Task AfterMapAsync(User source, UserDto target, CancellationToken ct = default)
    {
        // Async enrichment
        target.ProfilePictureUrl = await GetProfilePictureAsync(source.Id, ct);
        target.ReputationScore = await CalculateReputationAsync(source.Email, ct);
    }
}
```

### Async Instance Hooks with DI

```csharp
using DotNetBrightener.Mapper.Mapping;

public class UserCompleteAsyncHook : IMapHooksConfigurationAsyncInstance<User, UserDto>
{
    private readonly IUserValidator _validator;
    private readonly IProfileService _profileService;
    
    public UserCompleteAsyncHook(IUserValidator validator, IProfileService profileService)
    {
        _validator = validator;
        _profileService = profileService;
    }
    
    public async Task BeforeMapAsync(User source, UserDto target, CancellationToken ct = default)
    {
        await _validator.ValidateAsync(source, ct);
        target.MappedAt = DateTime.UtcNow;
    }
    
    public async Task AfterMapAsync(User source, UserDto target, CancellationToken ct = default)
    {
        target.ProfileUrl = await _profileService.GetProfileUrlAsync(source.Id, ct);
    }
}
```

## Execution Order

When using both Configuration and Hooks, the execution order is:

1. **BeforeMap** (from `BeforeMapConfiguration`)
2. **Property Mapping** (automatic)
3. **Configuration.Map** (from `Configuration`)
4. **AfterMap** (from `AfterMapConfiguration`)

```csharp
// Execution timeline:
// 1. BeforeMapConfig.BeforeMap(source, target)  <- target is empty
// 2. target.Id = source.Id                       <- automatic mapping
// 3. target.Name = source.Name                   <- automatic mapping
// 4. CustomConfig.Map(source, target)            <- Configuration (if specified)
// 5. AfterMapConfig.AfterMap(source, target)     <- target is fully populated
```

## Common Patterns

### Validation Pattern

```csharp
public class OrderValidationHook : IBeforeMapConfiguration<Order, OrderDto>
{
    public static void BeforeMap(Order source, OrderDto target)
    {
        if (source.Items == null || !source.Items.Any())
            throw new BusinessRuleException("Order must have at least one item");
        
        if (source.Total <= 0)
            throw new BusinessRuleException("Order total must be positive");
    }
}
```

### Audit Pattern

```csharp
public class AuditBeforeHook<TSource, TDto> : IBeforeMapConfiguration<TSource, TDto>
    where TDto : IAuditable
{
    public static void BeforeMap(TSource source, TDto target)
    {
        target.MappedAt = DateTime.UtcNow;
        target.MappedBy = Thread.CurrentPrincipal?.Identity?.Name ?? "System";
    }
}
```

### Sanitization Pattern

```csharp
public class UserSanitizationHook : IAfterMapConfiguration<User, UserDto>
{
    public static void AfterMap(User source, UserDto target)
    {
        // Sanitize PII for logging/display
        target.Email = MaskEmail(target.Email);
        target.Phone = MaskPhone(target.Phone);
    }
    
    private static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return email;
        var atIndex = email.IndexOf('@');
        if (atIndex < 2) return "***" + email.Substring(atIndex);
        return email.Substring(0, 2) + "***" + email.Substring(atIndex);
    }
}
```

### Enrichment Pattern

```csharp
public class ProductEnrichmentHook : IAfterMapConfigurationAsyncInstance<Product, ProductDto>
{
    private readonly IPricingService _pricing;
    private readonly IInventoryService _inventory;
    
    public ProductEnrichmentHook(IPricingService pricing, IInventoryService inventory)
    {
        _pricing = pricing;
        _inventory = inventory;
    }
    
    public async Task AfterMapAsync(Product source, ProductDto target, CancellationToken ct = default)
    {
        // Enrich with external data
        target.CurrentPrice = await _pricing.GetCurrentPriceAsync(source.Id, ct);
        target.StockLevel = await _inventory.GetStockLevelAsync(source.Id, ct);
        target.IsAvailable = target.StockLevel > 0;
    }
}
```

## Interface Reference

### Static Interfaces

| Interface | Method Signature |
|-----------|-----------------|
| `IBeforeMapConfiguration<TSource, TTarget>` | `static void BeforeMap(TSource source, TTarget target)` |
| `IAfterMapConfiguration<TSource, TTarget>` | `static void AfterMap(TSource source, TTarget target)` |
| `IMapHooksConfiguration<TSource, TTarget>` | Both methods above |
| `IBeforeMapConfigurationAsync<TSource, TTarget>` | `static Task BeforeMapAsync(TSource source, TTarget target, CancellationToken ct)` |
| `IAfterMapConfigurationAsync<TSource, TTarget>` | `static Task AfterMapAsync(TSource source, TTarget target, CancellationToken ct)` |
| `IMapHooksConfigurationAsync<TSource, TTarget>` | Both async methods above |

### Instance Interfaces (with DI)

| Interface | Method Signature |
|-----------|-----------------|
| `IBeforeMapConfigurationInstance<TSource, TTarget>` | `void BeforeMap(TSource source, TTarget target)` |
| `IAfterMapConfigurationInstance<TSource, TTarget>` | `void AfterMap(TSource source, TTarget target)` |
| `IMapHooksConfigurationInstance<TSource, TTarget>` | Both methods above |
| `IBeforeMapConfigurationAsyncInstance<TSource, TTarget>` | `Task BeforeMapAsync(TSource source, TTarget target, CancellationToken ct)` |
| `IAfterMapConfigurationAsyncInstance<TSource, TTarget>` | `Task AfterMapAsync(TSource source, TTarget target, CancellationToken ct)` |
| `IMapHooksConfigurationAsyncInstance<TSource, TTarget>` | Both async methods above |

## Best Practices

1. **Keep hooks focused**: Each hook should have a single responsibility
2. **Avoid side effects in BeforeMap**: Only validate and set defaults
3. **Use AfterMap for derived values**: Computed properties should be set in AfterMap
4. **Prefer async for I/O**: Use async hooks when calling external services
5. **Handle nulls**: Check for null references, especially in AfterMap
6. **Consider performance**: Hooks run on every mapping operation
7. **Test hooks independently**: Write unit tests for your hook implementations

## Limitations

- **Projection expressions**: Hooks are not called when using `Projection` (LINQ/EF queries)
- **Circular references**: Be careful not to create infinite loops in hooks
- **Thread safety**: Static hooks must be thread-safe

## See Also

- [Custom Mapping](04_CustomMapping.md)
- [Async Mapping](08_AsyncMapping.md)
- [Attribute Reference](03_AttributeReference.md)
