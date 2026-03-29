# DotNetBrightener.Mapper.Dashboard

Copyright © 2017 - 2026 Vampire Coder (formerly DotnetBrightener)

Interactive web dashboard that auto-discovers and visualizes every `[MappingTarget<T>]` usage in your ASP.NET Core app. Think of it as a Swagger UI, but for your compile-time object mappings instead of HTTP endpoints.

## Why?

When a project accumulates dozens of source entities mapped to many DTOs, it gets hard to track which properties are included, excluded, renamed, or nested. This dashboard gives you a single page to inspect all of that at a glance — no XML docs or code navigation needed.

## Install

```bash
dotnet add package DotNetBrightener.Mapper.Dashboard
```

## Get Running in 30 Seconds

```csharp
// Program.cs
using DotNetBrightener.Mapper.Dashboard;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDotNetBrightenerMapperDashboard();

var app = builder.Build();
app.MapDotNetBrightenerMapperDashboard();
app.Run();
```

Fire up the app and open **`https://localhost:5001/dnb-mapper`**.

## What You See

| Section | Details |
|---------|---------|
| **Source type cards** | Expandable cards per source entity — click to reveal properties and all targets |
| **Source properties** | Name, friendly type (`int?`, `List<T>`), modifiers (Nullable, Required, Init, Collection) |
| **Target cards** | Each `[MappingTarget]` DTO: type kind, feature flags, excluded/included props, member count |
| **Feature badges** | Green check / red X for Constructor, Projection, ToSource |
| **Search bar** | Filter by source or target name |
| **Dark mode** | Follows your OS setting unless overridden |

## Tweaking the Defaults

```csharp
builder.Services.AddDotNetBrightenerMapperDashboard(options =>
{
    // Where the dashboard lives (default: "/dnb-mapper")
    options.RoutePrefix = "/mapper-inspector";

    // Page title shown in the browser tab
    options.Title = "Acme Corp — Mapping Inspector";

    // Brand color for headings and badges
    options.AccentColor = "#0ea5e9";   // sky blue

    // Force dark mode on first load (default: follows OS)
    options.DefaultDarkMode = true;

    // Turn off the JSON API if you only need the HTML view
    options.EnableJsonApi = false;
});
```

### Lock It Behind Auth

```csharp
builder.Services.AddDotNetBrightenerMapperDashboard(options =>
{
    options.RequireAuthentication = true;
    options.AuthenticationPolicy = "AdminOnly";   // ties into your existing policy
});
```

### Scan Extra Assemblies

The dashboard auto-scans the entry assembly + its references. Point it at more:

```csharp
builder.Services.AddDotNetBrightenerMapperDashboard(options =>
{
    options.AdditionalAssemblies.Add(typeof(ExternalDto).Assembly);
});
```

### Include System Assemblies

Off by default to keep things fast. Flip it on if you map from `Microsoft.*` / `System.*` types:

```csharp
builder.Services.AddDotNetBrightenerMapperDashboard(options =>
{
    options.IncludeSystemAssemblies = true;
});
```

## HTTP Endpoints

| URL | What it returns |
|-----|-----------------|
| `GET /dnb-mapper` | Full HTML dashboard |
| `GET /dnb-mapper/api/dnb-mapping-types` | Machine-readable JSON of every discovered mapping |

## JSON API Shape

A quick taste — the real response includes every source member and target member:

```json
[
  {
    "sourceTypeName": "MyApp.Models.User",
    "sourceTypeSimpleName": "User",
    "sourceTypeNamespace": "MyApp.Models",
    "sourceMembers": [
      { "name": "Id", "typeName": "int", "isNullable": false, "isRequired": false, "isCollection": false }
    ],
    "targets": [
      {
        "targetTypeName": "MyApp.DTOs.UserDto",
        "targetTypeSimpleName": "UserDto",
        "typeKind": "record",
        "hasConstructor": true,
        "hasProjection": true,
        "hasToSource": false,
        "excludedProperties": ["PasswordHash"],
        "members": [
          { "name": "Id", "typeName": "int", "isNullable": false, "isInitOnly": true }
        ]
      }
    ]
  }
]
```

## Real-World Example

```csharp
// Entity
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public List<Order> Orders { get; set; }
}

// Full DTO minus the hash
[MappingTarget<User>(nameof(User.PasswordHash))]
public partial record UserDto;

// Minimal DTO for dropdowns / lists
[MappingTarget<User>(Include = new[] { nameof(User.Id), nameof(User.Name) })]
public partial record UserSummaryDto;
```

Open the dashboard and you'll see the **User** card with both targets: `UserDto` (4 members, PasswordHash excluded) and `UserSummaryDto` (2 members, Id + Name only).

## Requirements

- .NET 10.0+ with ASP.NET Core
- A project that references `DotNetBrightener.Mapper` (the source generator)

## Ecosystem

| Package | Purpose |
|---------|---------|
| [DotNetBrightener.Mapper](https://www.nuget.org/packages/DotNetBrightener.Mapper) | Roslyn source generator — produces the mapping code at compile time |
| [DotNetBrightener.Mapper.Attributes](https://www.nuget.org/packages/DotNetBrightener.Mapper.Attributes) | `[MappingTarget<T>]`, `[MapFrom]`, `[GenerateDtos]` — the attributes the generator reads |
| [DotNetBrightener.Mapper.Mapping](https://www.nuget.org/packages/DotNetBrightener.Mapper.Mapping) | Runtime helpers — `.ToTarget()`, `.SelectTargets()`, `.ToSource()` extension methods |
| [DotNetBrightener.Mapper.Mapping.EFCore](https://www.nuget.org/packages/DotNetBrightener.Mapper.Mapping.EFCore) | EF Core integration — use generated projections in LINQ queries |
