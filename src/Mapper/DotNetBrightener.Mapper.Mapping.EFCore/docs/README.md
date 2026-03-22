# DotNetBrightener.Mapper.Mapping.EFCore

`DotNetBrightener.Mapper.Mapping.EFCore` adds Entity Framework Core helpers on top of `DotNetBrightener.Mapper.Mapping`.

It covers two main scenarios:

- SQL-translatable projection from `IQueryable<TSource>` to generated mapping targets
- custom async mapping for cases that cannot be translated to SQL

## Install

```bash
dotnet add package DotNetBrightener.Mapper.Mapping.EFCore
```

## Namespaces

```csharp
using DotNetBrightener.Mapper;
using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Mapping.Configurations;
using DotNetBrightener.Mapper.Mapping.EFCore;
```

## Generated Target Example

```csharp
[MappingTarget<User>(GenerateToSource = true)]
public partial class UserDto;
```

## Query Projection API

Use these methods when your mapping can be expressed through the generated `Projection` expression.

Available methods:

- `ToTargetsAsync<TSource, TTarget>(CancellationToken cancellationToken = default)`
- `ToTargetsAsync<TTarget>(CancellationToken cancellationToken = default)`
- `FirstAsync<TSource, TTarget>(CancellationToken cancellationToken = default)`
- `FirstAsync<TTarget>(CancellationToken cancellationToken = default)`
- `SingleAsync<TSource, TTarget>(CancellationToken cancellationToken = default)`
- `SingleAsync<TTarget>(CancellationToken cancellationToken = default)`

Example:

```csharp
var users = await dbContext.Users
    .Where(x => x.IsActive)
    .ToTargetsAsync<User, UserDto>();

var inferredUsers = await dbContext.Users
    .Where(x => x.IsActive)
    .ToTargetsAsync<UserDto>();

var firstUser = await dbContext.Users
    .Where(x => x.Id == id)
    .FirstAsync<UserDto>();

var singleUser = await dbContext.Users
    .Where(x => x.Email == email)
    .SingleAsync<User, UserDto>();
```

### Streaming

For streaming, use the projection API from `DotNetBrightener.Mapper.Mapping` and then switch to EF Core async enumeration:

```csharp
await foreach (var user in dbContext.Users
    .Where(x => x.IsActive)
    .SelectTarget<UserDto>()
    .AsAsyncEnumerable())
{
    Console.WriteLine(user.Email);
}
```

Call `SelectTarget(...)` before `AsAsyncEnumerable()` so the projection is translated to SQL.

## Custom Async Mapping

Use the custom async overloads when mapping needs extra logic after the normal property copy, such as:

- injected services
- API calls
- non-SQL type conversions
- other async work

These methods materialize the query first, then apply custom async mapping.

Available methods:

- `ToTargetsAsync<TSource, TTarget>(IMappingConfigurationAsyncInstance<TSource, TTarget> mapper, CancellationToken cancellationToken = default)`
- `ToTargetsAsync<TSource, TTarget, TAsyncMapper>(CancellationToken cancellationToken = default)`
- `FirstAsync<TSource, TTarget>(IMappingConfigurationAsyncInstance<TSource, TTarget> mapper, CancellationToken cancellationToken = default)`
- `FirstAsync<TSource, TTarget, TAsyncMapper>(CancellationToken cancellationToken = default)`
- `SingleAsync<TSource, TTarget>(IMappingConfigurationAsyncInstance<TSource, TTarget> mapper, CancellationToken cancellationToken = default)`
- `SingleAsync<TSource, TTarget, TAsyncMapper>(CancellationToken cancellationToken = default)`

### Instance Mapper With DI

```csharp
public sealed class UserMapper : IMappingConfigurationAsyncInstance<User, UserDto>
{
    private readonly IAvatarService _avatarService;

    public UserMapper(IAvatarService avatarService)
    {
        _avatarService = avatarService;
    }

    public async Task MapAsync(User source, UserDto target, CancellationToken cancellationToken = default)
    {
        target.AvatarUrl = await _avatarService.GetAvatarUrlAsync(source.Id, cancellationToken);
    }
}
```

```csharp
var users = await dbContext.Users
    .Where(x => x.IsActive)
    .ToTargetsAsync<User, UserDto>(userMapper);
```

### Static Async Mapper

```csharp
public sealed class UserMapper : IMappingConfigurationAsync<User, UserDto>
{
    public static Task MapAsync(User source, UserDto target, CancellationToken cancellationToken = default)
    {
        target.DisplayName = $"{source.FirstName} {source.LastName}";
        return Task.CompletedTask;
    }
}
```

```csharp
var users = await dbContext.Users
    .Where(x => x.IsActive)
    .ToTargetsAsync<User, UserDto, UserMapper>();
```

## Update Existing Entities From Targets

This package also provides EF Core change-tracking helpers for updating entities from target DTOs.

Available methods:

- `UpdateFromTarget<TEntity, TTarget>(TTarget target, DbContext context)`
- `UpdateFromTargetAsync<TEntity, TTarget>(TTarget target, DbContext context, CancellationToken cancellationToken = default)`
- `UpdateFromTargetWithChanges<TEntity, TTarget>(TTarget target, DbContext context)`

Example:

```csharp
[MappingTarget<User>("PasswordHash", "CreatedAt", GenerateToSource = true)]
public partial class UpdateUserDto;

var entity = await dbContext.Users.FindAsync(id);
if (entity is null)
{
    return Results.NotFound();
}

entity.UpdateFromTarget(updateDto, dbContext);
await dbContext.SaveChangesAsync();
```

If you need to know which properties changed:

```csharp
var result = entity.UpdateFromTargetWithChanges(updateDto, dbContext);

if (result.ChangedProperties.Any())
{
    logger.LogInformation(
        "Updated user {UserId}: {ChangedProperties}",
        entity.Id,
        string.Join(", ", result.ChangedProperties));
}
```

## Notes

- The projection-based methods use generated projection expressions and remain SQL-translatable.
- The custom async mapper methods execute the EF Core query first, then run your custom mapping logic in memory.
- The update helpers only copy properties that exist on both the target and the entity.

## Related APIs

This package builds on the APIs from `DotNetBrightener.Mapper.Mapping`, including:

- `SelectTarget<TTarget>()`
- `SelectTarget<TSource, TTarget>()`
- `ToTarget(...)`
- `ToSource(...)`

Use those when you need provider-agnostic mapping helpers outside EF Core-specific async operations.
