# Security Infratructure Library

&copy; DotNet Brightener

## Installation

Run this in command line:

``` bash
dotnet add package DotNetBrightener.Infrastructure.Security
```

Or add the following to `.csproj` file

```xml
<PackageReference Include="DotNetBrightener.Infrastructure.Security" Version="2022.10.0" />
```

You should check the latest version from [Nuget Site](https://www.nuget.org)

## Adding Permission System

1. Register the Permission System to `IServiceCollection`

```cs 
serviceCollection.AddPermissionAuthorization();
```

2. Calling the following before application starts.

```cs
var app = builder.Build();

// call this before app.Run()
app.LoadAndValidatePermissions();

app.Run();
```

If you use legacy `Startup.cs` file then the `app.LoadAndValidatePermissions()` should be called in `Configure()` method

## Authorization Extensions

1. Authorize with `IAuthorizationService`

```cs
var authorizationResult = await IAuthorizationService.AuthorizePermissionAsync(user, permissionKey);
```

2. Authorize with `[PermissionAuthorize]` attribute

```cs

public class SomeController: Controller 
{
    [PermissionAuthorize("<your_permission_key>")]
    public async Task<IActionResult> SomeAction() 
    {
        // your action implementation
    }   
}

```

## Automatically Register Permissions

Given the following class for defining permissions

```cs

public class SomePermissionsList: AutomaticPermissionProvider
{
    /// <summary>
    ///     Description for Permission 1
    /// </summary>
    public const string Permission1 = "ThisIsKey.OfThePermission";

    /// <summary>
    ///     Description for Permission 2
    /// </summary>
    public const string Permission2 = "Permission.Permission2";
}
```

You can register the permissions defined in this class into service collection by doing the following:

```cs 
// with SomePermissionsList is the type of permission provider
serviceCollection.RegisterPermissionProvider<SomePermissionsList>();
```

The `app.LoadAndValidatePermissions()` call above should automatically register the permissions you defined in the class provided

## Minimal API Support

The library provides extension methods for Minimal API endpoints using `RequirePermission()` and `RequireAnyPermission()`.

### Basic Permission Authorization (ALL required)

User must have **ALL** specified permissions:

```csharp
// Single permission
app.MapGet("/api/users", GetUsers)
   .RequirePermission("UserManagement.View");

// Multiple permissions (ALL required)
app.MapDelete("/api/users/{id}", DeleteUser)
   .RequirePermission("UserManagement.Delete", "UserManagement.View");
```

### Any Permission Authorization (OR logic)

User must have **at least ONE** of the specified permissions:

```csharp
// Any one permission is sufficient
app.MapGet("/api/reports", GetReports)
   .RequireAnyPermission("Reports.View", "Reports.ViewAll", "Admin.Access");
```

### Resource-Based Authorization

For scenarios where authorization depends on a specific resource:

```csharp
// Single permission with resource
app.MapGet("/api/documents/{id}", async (int id, IDocumentService docService) =>
{
    var document = await docService.GetById(id);
    return Results.Ok(document);
}).RequirePermission("Document.View", documentResource);

// Multiple permissions for resource (ALL required)
app.MapPut("/api/documents/{id}", UpdateDocument)
   .RequirePermissionForResource(documentResource, "Document.Edit", "Document.View");

// Any permission for resource (OR logic)
app.MapGet("/api/projects/{id}", GetProject)
   .RequireAnyPermissionForResource(projectResource, "Project.View", "Project.ViewAll", "Admin.Access");
```

### Allow Anonymous

Use `[AllowAnonymous]` attribute on the endpoint delegate or `.AllowAnonymous()` to bypass permission checks:

```csharp
app.MapGet("/api/public/info", GetPublicInfo)
   .AllowAnonymous();
```

### Summary of Extension Methods

| Method | Logic | Resource Support |
|--------|-------|------------------|
| `RequirePermission(params string[])` | ALL required | No |
| `RequirePermission(string, object)` | Single permission | Yes |
| `RequirePermissionForResource(object, params string[])` | ALL required | Yes |
| `RequireAnyPermission(params string[])` | Any one | No |
| `RequireAnyPermissionForResource(object, params string[])` | Any one | Yes |