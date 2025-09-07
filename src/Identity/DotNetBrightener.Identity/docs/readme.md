# Identity Module for DotNet Brightener Framework Project

&copy; 2025 DotNet Brightener. All rights reserved.

This module provides identity management functionalities for DotNet Brightener Framework project. It includes user management, role management, permission management, and account management. It also provides authentication and authorization services.

## Installation

Run this in command line:

``` bash
dotnet add package DotNetBrightener.IdentityAuth
```

Or add the following to `.csproj` file

```xml
<PackageReference Include="DotNetBrightener.IdentityAuth" Version="2025.0.0" />
```

You should check the latest version from [Nuget Site](https://www.nuget.org)

## Usage

### Register the service

```csharp
serviceCollection.AddDotNetBrightenerIdentity()
                 .AddEntityFrameworkStores<YourDbContext>(options => {
                    options.UseSqlServer(connectionString);
                 });
```


