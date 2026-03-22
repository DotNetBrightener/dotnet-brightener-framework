# Analyzer Rules

The project includes Roslyn analyzers that provide real-time feedback in your IDE. These analyzers catch common mistakes and configuration issues at design-time, before you even compile your code.

## Quick Reference

| Rule ID | Severity | Category | Description |
|---------|----------|----------|-------------|
| [DNB001](#dnb001) | Error | Usage | Type must be annotated with [MappingTarget] |
| [DNB002](#dnb002) | Info | Performance | Consider using two-generic variant |
| [DNB003](#dnb003) | Error | Declaration | Missing partial keyword on [MappingTarget] type |
| [DNB004](#dnb004) | Error | Usage | Invalid property name in Exclude/Include |
| [DNB005](#dnb005) | Error | Usage | Invalid source type |
| [DNB006](#dnb006) | Error | Usage | Invalid Configuration type |
| [DNB007](#dnb007) | Warning | Usage | Invalid NestedTargetTypes type |
| [DNB008](#dnb008) | Warning | Performance | Circular reference risk |
| [DNB009](#dnb009) | Error | Usage | Both Include and Exclude specified |
| [DNB010](#dnb010) | Warning | Performance | Unusual MaxDepth value |
| [DNB011](#dnb011) | Error | Usage | [GenerateDtos] on non-class type |
| [DNB012](#dnb012) | Warning | Usage | Invalid ExcludeProperties |
| [DNB013](#dnb013) | Warning | Usage | No DTO types selected |
| [DNB014](#dnb014) | Error | Declaration | Missing partial keyword on [Flatten] type |
| [DNB015](#dnb015) | Error | Usage | Invalid source type in [Flatten] |
| [DNB016](#dnb016) | Warning | Performance | Unusual MaxDepth in [Flatten] |
| [DNB017](#dnb017) | Info | Usage | LeafOnly naming collision risk |
| [DNB022](#dnb022) | Warning | SourceTracking | Source entity structure changed |

---

## Extension Method Analyzers

### DNB001

**Type must be annotated with [MappingTarget]**

- **Severity**: Error
- **Category**: Usage

#### Description

When using extension methods like `ToTarget<T>()`, `ToSource<T>()`, `SelectTarget<T>()`, etc., the target type must be annotated with the `[MappingTarget]` attribute.

#### Bad Code

```csharp
// UserDto does NOT have [MappingTarget] attribute
public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

var dto = user.ToTarget<User, UserDto>(); // ❌ DNB001
```

#### Good Code

```csharp
[MappingTarget<User>]
public partial class UserDto { }

var dto = user.ToTarget<User, UserDto>(); // ✅ OK
```

---

### DNB002

**Consider using the two-generic variant for better performance**

- **Severity**: Info
- **Category**: Performance

#### Description

When using single-generic extension methods like `ToTarget<TTarget>()`, the library uses reflection to discover the source type. For better performance, use the two-generic variant `ToTarget<TSource, TTarget>()`.

#### Code Triggering Warning

```csharp
var dto = user.ToTarget<UserDto>(); // ℹ️ DNB002: Consider ToTarget<User, UserDto>()
```

#### Recommended Code

```csharp
var dto = user.ToTarget<User, UserDto>(); // ✅ Better performance
```

#### Impact

The performance difference is minimal (a few nanoseconds) but can add up in tight loops or high-throughput scenarios.

---

## [MappingTarget] Attribute Analyzers

### DNB003

**Type with [MappingTarget] attribute must be declared as partial**

- **Severity**: Error
- **Category**: Declaration

#### Description

Source generators require types to be `partial` so they can add generated members. Any type marked with `[MappingTarget]` must be declared as `partial`.

#### Bad Code

```csharp
[MappingTarget<User>]
public class UserDto { } // ❌ DNB003: Missing 'partial' keyword
```

#### Good Code

```csharp
[MappingTarget<User>]
public partial class UserDto { } // ✅ OK
```

---

### DNB004

**Property name does not exist in source type**

- **Severity**: Error
- **Category**: Usage

#### Description

Property names specified in `Exclude` or `Include` parameters must exist in the source type.

#### Bad Code

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
}

[MappingTarget<User>("PasswordHash")] // ❌ DNB004: User doesn't have PasswordHash
public partial class UserDto { }
```

#### Good Code

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string PasswordHash { get; set; }
}

[MappingTarget<User>("PasswordHash")] // ✅ OK
public partial class UserDto { }
```

---

### DNB005

**Source type is not accessible or does not exist**

- **Severity**: Error
- **Category**: Usage

#### Description

The source type specified in the `[MappingTarget]` attribute must be a valid, accessible type.

#### Bad Code

```csharp
[MappingTarget<NonExistentType>] // ❌ DNB005
public partial class UserDto { }
```

#### Good Code

```csharp
[MappingTarget<User>] // ✅ OK
public partial class UserDto { }
```

---

### DNB006

**Configuration type does not implement required interface**

- **Severity**: Error
- **Category**: Usage

#### Description

Configuration types must implement `IMappingConfiguration<TSource, TTarget>`, `IMappingConfigurationAsync<TSource, TTarget>`, or provide a static `Map` method.

#### Bad Code

```csharp
public class UserMapper // ❌ No interface, no Map method
{
    public void DoSomething(User source, UserDto target) { }
}

[MappingTarget<User>(Configuration = typeof(UserMapper))] // ❌ DNB006
public partial class UserDto { }
```

#### Good Code

```csharp
// Option 1: Implement interface
public class UserMapper : IMappingConfiguration<User, UserDto>
{
    public static void Map(User source, UserDto target)
    {
        target.FullName = $"{source.FirstName} {source.LastName}";
    }
}

// Option 2: Provide static Map method
public class UserMapper
{
    public static void Map(User source, UserDto target)
    {
        target.FullName = $"{source.FirstName} {source.LastName}";
    }
}

[MappingTarget<User>(Configuration = typeof(UserMapper))] // ✅ OK
public partial class UserDto { }
```

---

### DNB007

**Nested target type is not marked with [MappingTarget] attribute**

- **Severity**: Warning
- **Category**: Usage

#### Description

All types specified in the `NestedTargetTypes` array must be marked with the `[MappingTarget]` attribute.

#### Bad Code

```csharp
public class AddressDto { } // ❌ Missing [MappingTarget] attribute

[MappingTarget<User>(NestedTargetTypes = [typeof(AddressDto)])] // ⚠️ DNB007
public partial class UserDto { }
```

#### Good Code

```csharp
[MappingTarget<Address>]
public partial class AddressDto { }

[MappingTarget<User>(NestedTargetTypes = [typeof(AddressDto)])] // ✅ OK
public partial class UserDto { }
```

---

### DNB008

**Potential stack overflow with circular references**

- **Severity**: Warning
- **Category**: Performance

#### Description

When `MaxDepth` is set to 0 (unlimited) and `PreserveReferences` is `false`, circular references in object graphs can cause stack overflow exceptions.

#### Bad Code

```csharp
[MappingTarget<User>(MaxDepth = 0,
    PreserveReferences = false,
    NestedTargetTypes = [typeof(CompanyDto)])] // ⚠️ DNB008
public partial class UserDto { }
```

#### Good Code

```csharp
// Option 1: Enable PreserveReferences (default)
[MappingTarget<User>(NestedTargetTypes = [typeof(CompanyDto)])] // ✅ OK (PreserveReferences defaults to true)

// Option 2: Set MaxDepth limit
[MappingTarget<User>(MaxDepth = 5,
    NestedTargetTypes = [typeof(CompanyDto)])] // ✅ OK

// Option 3: Both
[MappingTarget<User>(MaxDepth = 10,
    PreserveReferences = true,
    NestedTargetTypes = [typeof(CompanyDto)])] // ✅ OK (safest)
```

---

### DNB009

**Cannot specify both Include and Exclude**

- **Severity**: Error
- **Category**: Usage

#### Description

The `Include` and `Exclude` parameters are mutually exclusive. Use either `Include` to whitelist properties or `Exclude` to blacklist properties, but not both.

#### Bad Code

```csharp
[MappingTarget<User>(nameof(User.PasswordHash),  // Exclude parameter
    Include = [nameof(User.Id), nameof(User.Name)])] // ❌ DNB009: Can't use both
public partial class UserDto { }
```

#### Good Code

```csharp
// Option 1: Exclude approach
[MappingTarget<User>(nameof(User.PasswordHash), nameof(User.SecretKey))] // ✅ OK
public partial class UserDto { }

// Option 2: Include approach
[MappingTarget<User>(Include = [nameof(User.Id), nameof(User.Name), nameof(User.Email)])] // ✅ OK
public partial class UserDto { }
```

---

### DNB010

**MaxDepth value is unusual**

- **Severity**: Warning
- **Category**: Performance

#### Description

MaxDepth values should typically be between 1 and 10 for most scenarios. Negative values are invalid, and values above 100 may indicate a configuration error.

#### Code Triggering Warning

```csharp
[MappingTarget<User>(MaxDepth = -1)] // ⚠️ DNB010: Negative
[MappingTarget<User>(MaxDepth = 500)] // ⚠️ DNB010: Too large
```

#### Good Code

```csharp
[MappingTarget<User>(MaxDepth = 5)] // ✅ OK
[MappingTarget<User>(MaxDepth = 10)] // ✅ OK (default)
```

---

## [GenerateDtos] Attribute Analyzers

### DNB011

**[GenerateDtos] can only be applied to classes**

- **Severity**: Error
- **Category**: Usage

#### Description

The `[GenerateDtos]` and `[GenerateAuditableDtos]` attributes are designed for class types and cannot be applied to structs, interfaces, or other type kinds.

#### Bad Code

```csharp
[GenerateDtos(DtoTypes.All)]
public struct Product { } // ❌ DNB011: Can't use on struct
```

#### Good Code

```csharp
[GenerateDtos(DtoTypes.All)]
public class Product { } // ✅ OK
```

---

### DNB012

**Excluded property does not exist**

- **Severity**: Warning
- **Category**: Usage

#### Description

Properties specified in `ExcludeProperties` should exist in the source type.

#### Bad Code

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
}

[GenerateDtos(DtoTypes.All,
    ExcludeProperties = ["InternalNotes"])] // ⚠️ DNB012: Doesn't exist
public class Product { }
```

#### Good Code

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string InternalNotes { get; set; }
}

[GenerateDtos(DtoTypes.All,
    ExcludeProperties = ["InternalNotes"])] // ✅ OK
public class Product { }
```

---

### DNB013

**No DTO types selected for generation**

- **Severity**: Warning
- **Category**: Usage

#### Description

Setting `Types` to `DtoTypes.None` will not generate any DTOs.

#### Bad Code

```csharp
[GenerateDtos(Types = DtoTypes.None)] // ⚠️ DNB013: No DTOs will be generated
public class Product { }
```

#### Good Code

```csharp
[GenerateDtos(Types = DtoTypes.All)] // ✅ OK
public class Product { }

// Or specify specific types
[GenerateDtos(Types = DtoTypes.Create | DtoTypes.Update | DtoTypes.Response)]
public class Product { }
```

---

## [Flatten] Attribute Analyzers

### DNB014

**Type with [Flatten] attribute must be declared as partial**

- **Severity**: Error
- **Category**: Declaration

#### Description

Similar to `[MappingTarget]`, types marked with `[Flatten]` must be `partial`.

#### Bad Code

```csharp
[Flatten(typeof(Person))]
public class PersonFlat { } // ❌ DNB014
```

#### Good Code

```csharp
[Flatten(typeof(Person))]
public partial class PersonFlat { } // ✅ OK
```

---

### DNB015

**Source type is not accessible or does not exist**

- **Severity**: Error
- **Category**: Usage

#### Description

The source type specified in the `[Flatten]` attribute must be valid and accessible.

#### Bad Code

```csharp
[Flatten(typeof(NonExistentType))] // ❌ DNB015
public partial class PersonFlat { }
```

#### Good Code

```csharp
[Flatten(typeof(Person))] // ✅ OK
public partial class PersonFlat { }
```

---

### DNB016

**MaxDepth value is unusual**

- **Severity**: Warning
- **Category**: Performance

#### Description

For flatten scenarios, MaxDepth values should typically be between 1 and 5. Values above 10 may cause excessive property generation.

#### Code Triggering Warning

```csharp
[Flatten(typeof(Person), MaxDepth = -1)] // ⚠️ DNB016: Negative
[Flatten(typeof(Person), MaxDepth = 50)] // ⚠️ DNB016: Too large
```

#### Good Code

```csharp
[Flatten(typeof(Person), MaxDepth = 3)] // ✅ OK (default)
[Flatten(typeof(Person), MaxDepth = 5)] // ✅ OK
```

---

### DNB017

**LeafOnly naming strategy may cause property name collisions**

- **Severity**: Info
- **Category**: Usage

#### Description

Using `FlattenNamingStrategy.LeafOnly` can cause name collisions when multiple nested objects have properties with the same name. Consider using the `Prefix` strategy instead.

#### Code Triggering Warning

```csharp
[Flatten(typeof(Person),
    NamingStrategy = FlattenNamingStrategy.LeafOnly)] // ℹ️ DNB017
public partial class PersonFlat { }
```

#### Potential Issue

```csharp
public class Person
{
    public Address HomeAddress { get; set; }
    public Address WorkAddress { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
}

// With LeafOnly, both addresses map to "Street" and "City" → collision!
```

#### Recommended Code

```csharp
[Flatten(typeof(Person),
    NamingStrategy = FlattenNamingStrategy.Prefix)] // ✅ Better
public partial class PersonFlat { }

// Generates: HomeAddressStreet, HomeAddressCity, WorkAddressStreet, WorkAddressCity
```

---

## Source Signature Analyzers

### DNB022

**Source entity structure changed**

- **Severity**: Warning
- **Category**: SourceTracking

#### Description

When you set `SourceSignature` on a `[MappingTarget]` attribute, the analyzer computes a hash of the source type's properties and compares it to the stored signature. This warning is raised when the source entity's structure changes.

#### Code Triggering Warning

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }  // New property added
}

[MappingTarget<User>(SourceSignature = "oldvalue")]  // ⚠️ DNB022
public partial class UserDto { }
```

#### Resolution

Use the provided code fix to update the signature, or manually update it:

```csharp
[MappingTarget<User>(SourceSignature = "newvalue")]  // ✅ OK
public partial class UserDto { }
```

#### Notes

- The signature is an 8-character hash computed from property names and types
- Respects `Include`/`Exclude` filters when computing the signature
- A code fix provider automatically offers to update the signature
- See [Source Signature Change Tracking](16_SourceSignature.md) for details

---

## Suppressing Analyzer Rules

If you need to suppress a specific analyzer rule, you can use:

### In Code

```csharp
#pragma warning disable DNB002
var dto = user.ToTarget<UserDto>();
#pragma warning restore DNB002
```

### In .editorconfig

```ini
[*.cs]
dotnet_diagnostic.DNB002.severity = none
```

### For Entire Project

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);DNB002</NoWarn>
</PropertyGroup>
```

---

## Configuration

All analyzers are enabled by default. You can configure their severity in your `.editorconfig` file:

```ini
[*.cs]

# Set a rule to error
dotnet_diagnostic.DNB007.severity = error

# Set a rule to warning
dotnet_diagnostic.DNB002.severity = warning

# Disable a rule
dotnet_diagnostic.DNB017.severity = none
```

---

## See Also

- [Attribute Reference](03_AttributeReference.md)
- [Custom Mapping](04_CustomMapping.md)
- [Advanced Scenarios](06_AdvancedScenarios.md)
