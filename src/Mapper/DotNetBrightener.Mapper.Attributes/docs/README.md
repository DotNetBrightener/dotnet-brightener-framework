# DotNetBrightener.Mapper.Attributes

Copyright © 2017 - 2026 Vampire Coder (formerly DotnetBrightener)

`DotNetBrightener.Mapper.Attributes` contains the runtime attribute and enum definitions used by the `DotNetBrightener.Mapper` source generator ecosystem.

This package does not contain the generator itself. It provides the attribute types that your application code references.

## Install

If you are using the full mapper package, you typically do not need to install this package directly.

```bash
dotnet add package DotNetBrightener.Mapper
```

Install `DotNetBrightener.Mapper.Attributes` directly only if you specifically want the attribute definitions without bringing in the full generator package:

```bash
dotnet add package DotNetBrightener.Mapper.Attributes
```

## Namespace

```csharp
using DotNetBrightener.Mapper;
```

## Included Attributes

This package currently exposes:

- `MappingTargetAttribute<TSource>`
- `FlattenAttribute`
- `MapFromAttribute`
- `MapWhenAttribute`
- `GenerateDtosAttribute`
- `WrapperAttribute`

It also includes these enums:

- `DtoTypes`
- `OutputType`
- `FlattenNamingStrategy`

## `MappingTarget<TSource>`

Use `MappingTarget<TSource>` to generate a target type from a source type.

```csharp
[MappingTarget<User>(GenerateToSource = true)]
public partial class UserDto;
```

Common options include:

- `Exclude`
- `Include`
- `IncludeFields`
- `GenerateConstructor`
- `GenerateParameterlessConstructor`
- `GenerateProjection`
- `GenerateToSource`
- `Configuration`
- `BeforeMapConfiguration`
- `AfterMapConfiguration`
- `NestedTargetTypes`
- `FlattenTo`
- `NullableProperties`
- `PreserveInitOnlyProperties`
- `PreserveRequiredProperties`
- `PreserveReferences`
- `MaxDepth`
- `ConvertEnumsTo`
- `GenerateCopyConstructor`
- `GenerateEquality`
- `ChainToParameterlessConstructor`
- `CopyAttributes`
- `UseFullName`
- `SourceSignature`

Example with exclusions and nested targets:

```csharp
[MappingTarget<Order>(
    nameof(Order.InternalNotes),
    GenerateToSource = true,
    NestedTargetTypes = [typeof(OrderItemDto), typeof(AddressDto)])]
public partial class OrderDto;
```

## `Flatten`

Use `Flatten` to generate flattened output types from nested object graphs.

```csharp
[Flatten(typeof(Order))]
public partial class OrderExportRow;
```

Useful options include:

- `Exclude`
- `MaxDepth`
- `NamingStrategy`
- `IncludeFields`
- `GenerateParameterlessConstructor`
- `GenerateProjection`
- `UseFullName`
- `IgnoreNestedIds`
- `IgnoreForeignKeyClashes`
- `IncludeCollections`

## `MapFrom`

Use `MapFrom` on target members to map from a different source member or expression.

```csharp
[MappingTarget<User>]
public partial class UserDto
{
    [MapFrom(nameof(User.DisplayName))]
    public string Name { get; set; } = string.Empty;

    [MapFrom("FirstName + \" \" + LastName", Reversible = false)]
    public string FullName { get; set; } = string.Empty;
}
```

Key options:

- `Source`
- `Reversible`
- `IncludeInProjection`

## `MapWhen`

Use `MapWhen` to conditionally map a target member.

```csharp
[MappingTarget<Order>]
public partial class OrderDto
{
    [MapWhen("Status == OrderStatus.Completed")]
    public DateTime? CompletedAt { get; set; }
}
```

Key options:

- `Condition`
- `Default`
- `IncludeInProjection`

## `GenerateDtos`

Use `GenerateDtos` to generate common DTO shapes from a model.

```csharp
[GenerateDtos(
    Types = DtoTypes.Create | DtoTypes.Update | DtoTypes.Response,
    OutputType = OutputType.Record,
    ExcludeAuditFields = true)]
public partial class User;
```

Supported configuration includes:

- `Types`
- `OutputType`
- `Namespace`
- `ExcludeProperties`
- `ExcludeAuditFields`
- `Prefix`
- `Suffix`
- `IncludeFields`
- `GenerateConstructors`
- `GenerateProjections`
- `UseFullName`

## `Wrapper`

Use `Wrapper` to generate a delegating wrapper around an existing source type instead of copying values into a separate target object.

```csharp
[Wrapper(typeof(User), nameof(User.PasswordHash))]
public partial class UserWrapper;
```

Supported configuration includes:

- constructor `sourceType`
- constructor `exclude`
- `Include`
- `IncludeFields`
- `ReadOnly`
- `NestedWrappers`
- `CopyAttributes`
- `UseFullName`

## AOT And Runtime Safety

This package is runtime-safe and lightweight because it contains only attribute and enum definitions. Keeping these definitions separate from the generator avoids pulling Roslyn dependencies into application runtime deployments, including AOT scenarios.

