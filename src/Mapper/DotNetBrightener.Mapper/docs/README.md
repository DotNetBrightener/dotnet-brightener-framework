# DotNetBrightener.Mapper

Copyright © 2017 - 2026 Vampire Coder (formerly DotnetBrightener)

`DotNetBrightener.Mapper` is a compile-time DTO and projection generator for .NET.

Define a target type once with `MappingTarget<TSource>`, and the generator produces the mapping surface for you:
- source constructor
- parameterless constructor when enabled
- `Projection` expression for LINQ and EF Core
- `ToSource()` for reverse mapping when enabled
- patch/update helpers through the mapping packages

The project is split into focused packages so you only install what you need.

## Packages

### Core generator

```bash
dotnet add package DotNetBrightener.Mapper
```

This package provides the source generator and analyzer support.

### Attributes only

```bash
dotnet add package DotNetBrightener.Mapper.Attributes
```

Use this when you only need the runtime attributes and enums.

### Mapping extensions

```bash
dotnet add package DotNetBrightener.Mapper.Mapping
```

This package adds:
- `ToTarget`
- `ToSource`
- `SelectTargets`
- `SelectSources`
- `SelectTarget`
- `ApplyTarget`
- `ApplyTargetWithChanges`
- custom mapping interfaces
- expression mapping helpers

### EF Core mapping extensions

```bash
dotnet add package DotNetBrightener.Mapper.Mapping.EFCore
```

This package adds EF Core-specific helpers such as:
- `ToTargetsAsync`
- `FirstAsync`
- `SingleAsync`
- `UpdateFromTarget`
- `UpdateFromTargetAsync`
- `UpdateFromTargetWithChanges`

## Quick Example

```csharp
using DotNetBrightener.Mapper;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

[MappingTarget<User>("Password", "CreatedAt", GenerateToSource = true)]
public partial class UserDto;
```

The generated `UserDto` includes the mapped properties from `User`, a constructor from `User`, a `Projection` expression, and `ToSource()` because `GenerateToSource = true`.

## Basic Mapping

With `DotNetBrightener.Mapper.Mapping` installed:

```csharp
using DotNetBrightener.Mapper.Mapping;

var dto = user.ToTarget<UserDto>();
var dtoFast = user.ToTarget<User, UserDto>();

var entity = dto.ToSource<User>();
var entityFast = dto.ToSource<UserDto, User>();

var dtos = users.SelectTargets<UserDto>().ToList();
var projected = query.SelectTarget<UserDto>();
```

## Nested Target Types

```csharp
public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Address HeadquartersAddress { get; set; } = null!;
}

[MappingTarget<Address>]
public partial record AddressDto;

[MappingTarget<Company>(NestedTargetTypes = [typeof(AddressDto)])]
public partial record CompanyDto;
```

Nested object and collection mappings are generated automatically when the nested target types are declared.

## Custom Mapping

Static mapper:

```csharp
using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Mapping.Configurations;

public class UserMapper : IMappingConfiguration<User, UserDto>
{
    public static void Map(User source, UserDto target)
    {
        target.FullName = $"{source.FirstName} {source.LastName}";
    }
}

[MappingTarget<User>(Configuration = typeof(UserMapper))]
public partial class UserDto
{
    public string FullName { get; set; } = string.Empty;
}
```

Instance mapper with DI:

```csharp
using DotNetBrightener.Mapper.Mapping.Configurations;

public class UserMapperWithServices : IMappingConfigurationAsyncInstance<User, UserDto>
{
    private readonly IProfileService _profileService;

    public UserMapperWithServices(IProfileService profileService)
    {
        _profileService = profileService;
    }

    public async Task MapAsync(User source, UserDto target, CancellationToken cancellationToken = default)
    {
        target.ProfileUrl = await _profileService.GetProfileUrlAsync(source.Id, cancellationToken);
    }
}
```

## Additional Attributes

The generator also supports:
- `MapFrom`
- `MapWhen`
- `Flatten`
- `GenerateDtos`
- `Wrapper`

Additional generation options include:
- `Include`
- `CopyAttributes`
- `NullableProperties`
- `ConvertEnumsTo`
- `GenerateEquality`
- `GenerateCopyConstructor`
- `SourceSignature`

## Documentation

- [Overview](01_Overview.md)
- [Quick start](02_QuickStart.md)
- [MappingTarget attribute reference](03_AttributeReference.md)
- [Extension methods](05_Extensions.md)
- [Advanced scenarios](06_AdvancedScenarios.md)
- [Generated code examples](07_WhatIsBeingGenerated.md)
- [Async mapping](08_AsyncMapping.md)
- [GenerateDtos](09_GenerateDtosAttribute.md)
- [Expression mapping](10_ExpressionMapping.md)
- [Flatten](11_FlattenAttribute.md)
- [Wrapper](14_WrapperAttribute.md)
- [MapFrom](15_MapFromAttribute.md)
- [Source signature tracking](16_SourceSignature.md)
- [MapWhen](17_MapWhen.md)
- [Inheritance mapping](18_InheritanceMapping.md)
- [Mapping hooks](19_MappingHooks.md)
- [Enum conversion](20_ConvertEnumsTo.md)

## Project Packages

- [DotNetBrightener.Mapper](https://www.nuget.org/packages/DotNetBrightener.Mapper#readme-body-tab)
- [DotNetBrightener.Mapper.Attributes](https://www.nuget.org/packages/DotNetBrightener.Mapper.Attributes#readme-body-tab)
- [DotNetBrightener.Mapper.Mapping](https://www.nuget.org/packages/DotNetBrightener.Mapper.Mapping#readme-body-tab)
- [DotNetBrightener.Mapper.Mapping.EFCore](https://www.nuget.org/packages/DotNetBrightener.Mapper.Mapping.EFCore#readme-body-tab)
- [DotNetBrightener.Mapper.Dashboard](https://www.nuget.org/packages/DotNetBrightener.Mapper.Dashboard#readme-body-tab)

