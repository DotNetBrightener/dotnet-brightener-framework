# DotNetBrightener.Mapper.Mapping

`DotNetBrightener.Mapper.Mapping` adds mapping helpers, custom mapper interfaces, and expression translation utilities for types generated with `MappingTarget<TSource>`.

## What This Package Includes

- In-memory mapping helpers for single objects, collections, and queryable projections
- Reverse mapping helpers for generating or updating source types from generated targets
- Custom mapping interfaces for synchronous, asynchronous, and hybrid mapping flows
- Before/after map hook interfaces for static and DI-friendly implementations
- Expression translation helpers for reusing predicates and selectors against generated target types

## Installation

```bash
dotnet add package DotNetBrightener.Mapper.Mapping
```

## Namespaces

```csharp
using DotNetBrightener.Mapper;
using DotNetBrightener.Mapper.Mapping;
```

## Basic Mapping

Define a generated target:

```csharp
[MappingTarget<User>(GenerateToSource = true)]
public partial class UserDto;
```

Map a single object:

```csharp
var dto = user.ToTarget<User, UserDto>();
var dto2 = user.ToTarget<UserDto>();
```

Map collections:

```csharp
var dtos = users.SelectTargets<User, UserDto>().ToList();
var dtos2 = users.SelectTargets<UserDto>().ToList();
```

Project an `IQueryable` with the generated projection:

```csharp
var query = dbContext.Users.SelectTarget<User, UserDto>();
var query2 = dbContext.Users.SelectTarget<UserDto>();
```

Map back to the source type:

```csharp
var entity = dto.ToSource<UserDto, User>();
var entity2 = ((object)dto).ToSource<User>();
```

Map a collection of targets back to sources:

```csharp
var entities = dtos.SelectSources<UserDto, User>().ToList();
var entities2 = ((IEnumerable)dtos).SelectSources<User>().ToList();
```

## Updating an Existing Source

Apply values from a generated target onto an existing source instance:

```csharp
var updatedUser = user.ApplyTarget<User, UserDto>(dto);
```

Track the changed properties:

```csharp
var result = user.ApplyTargetWithChanges<User, UserDto>(dto);

if (result.HasChanges)
{
    Console.WriteLine(string.Join(", ", result.ChangedProperties));
}
```

## Custom Mapping Interfaces

Use these interfaces when generated member-to-member mapping is not enough.

### Synchronous Mapping

Static mapper:

```csharp
public sealed class UserMapper : IMappingConfiguration<User, UserDto>
{
    public static void Map(User source, UserDto target)
    {
        target.FullName = $"{source.FirstName} {source.LastName}";
    }
}
```

Instance mapper:

```csharp
public sealed class UserMapper : IMappingConfigurationInstance<User, UserDto>
{
    public void Map(User source, UserDto target)
    {
        target.FullName = $"{source.FirstName} {source.LastName}";
    }
}
```

Generated targets can reference these mapper types from `MappingTarget<TSource>`, and instance mappers can also be passed directly to the sync extension helpers:

```csharp
var dto = user.ToTarget(userMapper);
var dtoFromCtor = user.ToTargetWithConstructor(userMapper);
var dtos = users.ToTargets(userMapper);
var dtoSync = user.ToTargetSync(userMapper);
```

### Before And After Hooks

Use hook interfaces to run logic before or after the generated mapping work:

- `IBeforeMapConfiguration<TSource, TTarget>`
- `IAfterMapConfiguration<TSource, TTarget>`
- `IMapHooksConfiguration<TSource, TTarget>`
- `IBeforeMapConfigurationInstance<TSource, TTarget>`
- `IAfterMapConfigurationInstance<TSource, TTarget>`
- `IMapHooksConfigurationInstance<TSource, TTarget>`

### Asynchronous Mapping

Static async mapper:

```csharp
public sealed class UserAsyncMapper : IMappingConfigurationAsync<User, UserDto>
{
    public static async Task MapAsync(User source, UserDto target, CancellationToken cancellationToken = default)
    {
        target.AvatarUrl = await avatarService.GetAvatarAsync(source.Id, cancellationToken);
    }
}
```

Instance async mapper:

```csharp
public sealed class UserAsyncMapper : IMappingConfigurationAsyncInstance<User, UserDto>
{
    public async Task MapAsync(User source, UserDto target, CancellationToken cancellationToken = default)
    {
        target.AvatarUrl = await avatarService.GetAvatarAsync(source.Id, cancellationToken);
    }
}
```

Available async helpers:

- `ToTargetAsync`
- `ToTargetWithConstructorAsync`
- `ToTargetsAsync`
- `ToTargetsParallelAsync`

Async hook interfaces are also available:

- `IBeforeMapConfigurationAsync<TSource, TTarget>`
- `IAfterMapConfigurationAsync<TSource, TTarget>`
- `IMapHooksConfigurationAsync<TSource, TTarget>`
- `IBeforeMapConfigurationAsyncInstance<TSource, TTarget>`
- `IAfterMapConfigurationAsyncInstance<TSource, TTarget>`
- `IMapHooksConfigurationAsyncInstance<TSource, TTarget>`

### Hybrid Mapping

Hybrid mappers combine fast synchronous work with asynchronous enrichment:

```csharp
public sealed class UserHybridMapper : IMappingConfigurationHybridInstance<User, UserDto>
{
    public void Map(User source, UserDto target)
    {
        target.FullName = $"{source.FirstName} {source.LastName}";
    }

    public async Task MapAsync(User source, UserDto target, CancellationToken cancellationToken = default)
    {
        target.AvatarUrl = await avatarService.GetAvatarAsync(source.Id, cancellationToken);
    }
}
```

Use `ToTargetHybridAsync(...)` with either static or instance hybrid mappers.

## Expression Mapping

Reuse source-type expressions against generated target types:

```csharp
Expression<Func<User, bool>> isActive = user => user.IsActive;
Expression<Func<UserDto, bool>> dtoFilter = isActive.MapToTarget<UserDto>();
```

Map selectors:

```csharp
Expression<Func<User, string>> byLastName = user => user.LastName;
Expression<Func<UserDto, string>> dtoSelector = byLastName.MapToTarget<UserDto, string>();
```

Map any lambda expression shape:

```csharp
var mapped = sourceExpression.MapToTargetGeneric<UserDto>();
```

Compose predicates:

```csharp
var combined = MappingExpressionExtensions.CombineWithAnd(isActive, isVerified);
var either = MappingExpressionExtensions.CombineWithOr(isActive, isVerified);
var inverted = isActive.Negate();
```

## Public API Summary

### Core Mapping Extensions

- `ToTarget<TSource, TTarget>(this TSource source)`
- `ToTarget<TTarget>(this object source)`
- `ToSource<TTarget, TSource>(this TTarget target)`
- `ToSource<TSource>(this object target)`
- `SelectTargets<TSource, TTarget>(this IEnumerable<TSource> source)`
- `SelectTargets<TTarget>(this IEnumerable source)`
- `SelectSources<TTarget, TSource>(this IEnumerable<TTarget> targets)`
- `SelectSources<TSource>(this IEnumerable targets)`
- `SelectTarget<TSource, TTarget>(this IQueryable<TSource> source)`
- `SelectTarget<TTarget>(this IQueryable source)`
- `ApplyTarget<TSource, TTarget>(this TSource source, TTarget target)`
- `ApplyTarget<TTarget>(this object source, TTarget target)`
- `ApplyTargetWithChanges<TSource, TTarget>(this TSource source, TTarget target)`

### Sync Custom Mapper Extensions

- `ToTarget(..., IMappingConfigurationInstance<TSource, TTarget> mapper)`
- `ToTargetWithConstructor(..., IMappingConfigurationInstance<TSource, TTarget> mapper)`
- `ToTargets(..., IMappingConfigurationInstance<TSource, TTarget> mapper)`
- `ToTargetSync(..., IMappingConfigurationInstance<TSource, TTarget> mapper)`

### Async And Hybrid Mapper Extensions

- `ToTargetAsync(...)`
- `ToTargetWithConstructorAsync(...)`
- `ToTargetsAsync(...)`
- `ToTargetsParallelAsync(...)`
- `ToTargetHybridAsync(...)`

### Expression Extensions

- `MapToTarget<TTarget>(this LambdaExpression sourcePredicate)`
- `MapToTarget<TTarget, TResult>(this LambdaExpression sourceSelector)`
- `MapToTargetGeneric<TTarget>(this LambdaExpression sourceExpression)`
- `CombineWithAnd<T>(params Expression<Func<T, bool>>[] predicates)`
- `CombineWithOr<T>(params Expression<Func<T, bool>>[] predicates)`
- `Negate<T>(this Expression<Func<T, bool>> predicate)`

## Related Packages

- `DotNetBrightener.Mapper.Attributes` for `MappingTarget<TSource>` and the mapping attributes
- `DotNetBrightener.Mapper` for the source generator that emits constructors, projections, and `ToSource`
- `DotNetBrightener.Mapper.Mapping.EFCore` for EF Core async query helpers and tracked source updates
