# What is Mapping Target Generation?

Mapping target generation is the process of defining **focused views** of a larger model at compile time.

Instead of manually writing separate DTOs, mappers, and projections, `DotNetBrightener.Mapper` lets you declare what you want to keep and generates everything else.

You can think of it like generating a **specific target view** of a larger model:

- The part you care about  
- Leaving the rest behind.

## Why Use Mapping Targets?

- Reduce duplication across DTOs, projections, and ViewModels
- Maintain strong typing with no runtime cost
- Stay DRY (Don't Repeat Yourself) without sacrificing performance
- Works seamlessly with LINQ providers like Entity Framework

## Example

Source model:

```csharp
public class User
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}
```

Define a target DTO:
```csharp
[MappingTarget<User>(nameof(User.Email))]
public partial class UserDto { }
```

You get:

- A mapped constructor
- A LINQ Expression Projection
- A partial class or record ready to extend
