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