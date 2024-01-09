# Code Generator for Centralized CRUD WebAPI in ASP.NET Core Applications
 
&copy; 2024 DotNet Brightener
 
 
# Getting Started

## Create new project

You can create the project in Visual Studio for your convenience. You can put everything into one single WebAPI project, but it's recommended not to do that, instead, you should separate the project into different libraries. The suggestion is as the following table

| Project Name | Project Type | Purpose | 
| -- | -- | -- |
| {YourProject}.WebAPI | WebAPI | The entry point of the application |
| {YourProject}.Core or {YourProject}.Entities | Class Library | The library that contains only the entities needed for your application |
| {YourProject}.Services | Class Library | The library that contains business logics and CRUD auto generated classes |
| {YourProject}.Database | Class Library | The library that holds the database context to communicate with the database |

If you like CLI, you can follow the instruction below.

1. Create a new .NET 8 WebAPI project:

```bash
dotnet new webapi -n {your-project-name} -f net8.0
dotnet new classlib -n {your-project-name}.Database -f net8.0
dotnet new sln --name {your-project-name}
dotnet sln {your-project-name}.sln add {your-project-name}\\{your-project-name}.csproj
dotnet sln {your-project-name}.sln add {your-project-name}.Database\\{your-project-name}.Database.csproj
```

2. If needed to separate the entity definitions to a different library, create new class library project 
```bash
dotnet new classlib -n {your-project-name}.Core -f net8.0
dotnet sln {your-project-name}.sln add {your-project-name}.Core\\{your-project-name}.Core.csproj
```

3. If needed to separate the service layer, create new class library project

```bash
dotnet new classlib -n {your-project-name}.Services -f net8.0
dotnet sln {your-project-name}.sln add {your-project-name}.Services\\{your-project-name}.Services.csproj
```

Now you can open the solution in Visual Studio.
## Add Generic CRUD and its Generator Library

### 1. Add CRUD libraries
Add the following packages to the projects, following the instruction below

| Package Name |  Project |
| -- | -- |
| DotNetBrightener.WebApi.GenericCRUD | Web API |
| DotNetBrightener.WebApi.GenericCRUD.Generator | Web API, Service |
| DotNetBrightener.Plugins.EventPubSub | Web API, Service |
| DotNetBrightener.Plugins.EventPubSub.DependencyInjection | Web API |
| DotNetBrightener.DataAccess.Abstractions | Core (Entity) |
| DotNetBrightener.DataAccess.EF | Database |

### 2. Update the settings to enable code generator
Open the `csproj` files and update the `<PackageReference>` of the `Generator` library with the following

```xml
<ItemGroup>
	<PackageReference Include="DotNetBrightener.WebApi.GenericCRUD.Generator"
					  Version="{current-version-of-the-library}"
					  OutputItemType="Analyzer"
					  ReferenceOutputAssembly="false" />
</ItemGroup>
```

The change is to add 
```xml 
	OutputItemType="Analyzer"
	ReferenceOutputAssembly="false"
```

to the XML tag, that enables the auto generate code for the projects.

## Create first entity

The entities must inherit from `BaseEntity` or `BaseEntityWithAuditInfo` from the `DotNetBrightener.DataAccess.Abstractions` library. Create any entity into the Core or Entity project.

Example: 

```cs
using DotNetBrightener.DataAccess.Models;
 
namespace CRUDWebApiWithGeneratorDemo.Core.Entities;

public class Product: BaseEntity
{
    [MaxLength(255)]
    public string Name { get; set; }

	// omitted code
}
```

The Entity/Core project should only contain the entities without any other logics

## Configure Code Generator for Service Project

At the root of Service project, create a class `CRUDDataServiceGeneratorRegistration.cs` with the following content

```cs
using CRUDWebApiWithGeneratorDemo.Core.Entities;
 
namespace CRUDWebApiWithGeneratorDemo.Services;
 
public class CRUDDataServiceGeneratorRegistration
{
    public List<Type> Entities =
    [
		// provide all entities that need to generate CRUD data service for
        typeof(Product),
        typeof(ProductCategory)
    ];
}
```

The name `CRUDDataServiceGeneratorRegistration` is strictly required as the generator library will look for that file in order to understand what to generate.

In the `Entities` list in the class, provide all the entities that needed to generate the CRUD data service interfaces and classes.

## Configure Code Generator for WebAPI Project

At the root of WebAPI project, create a class `CRUDWebApiGeneratorRegistration.cs` with the following content

```cs
public class CRUDWebApiGeneratorRegistration
{
	// reference the CRUDDataServiceGeneratorRegistration from Service project
    Type DataServiceRegistrationType = typeof(CRUDDataServiceGeneratorRegistration);
 
    public List<Type> Entities =
    [
		// provide all entities that need to generate CRUD API Controllers for
		// Some entities related to Authorization / Authentication / Security should be ignored
        typeof(Product),
        typeof(ProductCategory)
    ];
}
```

The name `CRUDWebApiGeneratorRegistration` is strictly required as the generator library will look for that file in order to understand what to generate.

In the `Entities` list in the class, provide all the entities that needed to generate the CRUD Web API Controllers for.

# Other configurations

In `Program.cs` (if you use minimal API), or `ConfigureService()` method in `Startup.cs` if you use traditional template of ASP.NET Web Application, you will need to add .NET Logging module and `EventPubSubService` from `DotNetBrightener` library, like the following:

```cs
// Program.cs
builder.Services.AddLogging();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEventPubSubService();

```

```cs
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
	// omitted code
	
	services.AddHttpContextAccessor();
	services.AddLogging();
	services.AddEventPubSubService();
	
	// omitted code
}

```

# Build and Run Project

As you followed the instruction so far, upon building and running the project, the needed classes will be automatically generated. By default, .NET WebAPI project includes the OpenAPI library, which will detect the available APIs and you can see the following when the project is launch:

![[swagger_screenshot.png]]

# Auto-Generated Web APIs

The generator will generate the following APIs for each entity

> GET /api/{entity-name} - Get all entities

This API accepts the following query string parameters

| Parameter | Type | Description |
| -- | -- | -- |
| columns | string[], separate by commas | The columns / fields of the entity to retrieve |
| pageSize | number | The number of records to return in one page, default is 20 |
| pageIndex | number | The index of the page to return, default is 0 |
| orderBy | string | The column to sort the result by, in ascending order. If the value starts with a hyphen (`-`) and followed by the column name, the result is sorted in descending order. |
| {column_name} | any | The filter expression to filter the result by the {column_name}. Eg: createdBy=user* will filter the result to return the records that have CreatedBy value starts with 'user'. |


> GET /api/{entity-name}/{id} - Get entity by id

This API accepts the following query string parameters

| Parameter | Type | Description |
| -- | -- | -- |
| columns | string[], separate by commas | The columns / fields of the entity to retrieve |

> POST /api/{entity-name} - Create new entity

> PUT /api/{entity-name}/{id} - Update entity by id

> DELETE /api/{entity-name}/{id} - Delete entity by id

> PUT /api/{entity-name}/{id}/undelete - Restore entity by id