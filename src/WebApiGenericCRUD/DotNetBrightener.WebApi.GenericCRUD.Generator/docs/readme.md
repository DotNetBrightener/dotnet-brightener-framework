Snippet

# Code Generator for Centralized CRUD WebAPI in ASP.NET Core Applications
 
&copy; 2025 DotNet Brightener. <admin@dotnetbrightener.com>
 
# Instruction
 
Most applications rely on CRUD operations. This tool aids in generating WebAPI controllers and DataService interfaces/classes using the [DotNetBrightener.WebApi.GenericCRUD](https://www.nuget.org/packages/DotNetBrightener.WebApi.GenericCRUD/) and [DotNetBrightener.DataAccess.Abstractions](https://www.nuget.org/packages/DotNetBrightener.DataAccess.Abstractions/) libraries. 
 
Specifically, [DotNetBrightener.WebApi.GenericCRUD](https://www.nuget.org/packages/DotNetBrightener.WebApi.GenericCRUD/) provides core CRUD functionalities, exposing them as WebAPI controllers, while [DotNetBrightener.DataAccess.Abstractions](https://www.nuget.org/packages/DotNetBrightener.DataAccess.Abstractions/) facilitates the database access layer performing these CRUD operations. 
 
Using this generator tool eliminates the need for manually implementing CRUD WebAPI controllers and Data Access Service classes/interfaces for each domain entity in your application. Upon building the application in Visual Studio, the necessary classes are automatically generated for your project, streamlining your workflow.
 
# Getting Started
 
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
 
### 1. Add needed libraries
 
Add the following packages to the projects, following the instruction below
 
| Package Name |  Project | Note
| -- | -- | -- |
| [DotNetBrightener.WebApi.GenericCRUD](https://www.nuget.org/packages/DotNetBrightener.WebApi.GenericCRUD/) | Web API |-|
| [DotNetBrightener.WebApi.GenericCRUD.Generator](https://www.nuget.org/packages/DotNetBrightener.WebApi.GenericCRUD.Generator/) | Web API, Service |-|
| [DotNetBrightener.Plugins.EventPubSub](https://www.nuget.org/packages/DotNetBrightener.Plugins.EventPubSub/) | Web API, Service |-|
| [DotNetBrightener.Plugins.EventPubSub.DependencyInjection](https://www.nuget.org/packages/DotNetBrightener.Plugins.EventPubSub.DependencyInjection/) | Web API |-|
| [DotNetBrightener.DataAccess.Abstractions](https://www.nuget.org/packages/DotNetBrightener.DataAccess.Abstractions/) | Core (Entity) |-|
| [DotNetBrightener.DataAccess.EF](https://www.nuget.org/packages/DotNetBrightener.DataAccess.EF/) | Database | If your project uses MSSQL as database provider |
| [DotNetBrightener.DataAccess.EF.PostgreSQL](https://www.nuget.org/packages/DotNetBrightener.DataAccess.EF.PostgreSQL/) | Database | If your project uses PostgreSQL as database provider |
 
### 2. Update the settings to enable code generator
 
Open the `csproj` file where you referenced the `DotNetBrightener.WebApi.GenericCRUD.Generator` library, and update the `<PackageReference>` of that package with the following
 
```xml
<ItemGroup>
	<PackageReference Include="DotNetBrightener.WebApi.GenericCRUD.Generator"
					  Version="{current-version-of-the-library}"
					  OutputItemType="Analyzer"
					  ReferenceOutputAssembly="false" />
</ItemGroup>
```
 
The change is to add 
 
```xml 
	OutputItemType="Analyzer"
	ReferenceOutputAssembly="false"
```
 
to the XML tag, which enables the auto generate code for the project.
 
## Create first entity
 
The entities must inherit from `BaseEntity` or `BaseEntityWithAuditInfo` from the `DotNetBrightener.DataAccess.Abstractions` library. Create any entity into the Core or Entity project.
 
Example: 
 
```csharp
using DotNetBrightener.DataAccess.Models;
 
namespace CRUDWebApiWithGeneratorDemo.Core.Entities;
 
public class Product: BaseEntity
{
    [MaxLength(255)]
    public string Name { get; set; }
 
	// omitted code
}
```
 
The Entity/Core project should only contain the entities without any other logics
 
## Configure Code Generator for Service Project
 
### Define auto-generate entities registration class
 
At the root of Service project, create a class `CRUDDataServiceGeneratorRegistration.cs` with the following content

```csharp
using CRUDWebApiWithGeneratorDemo.Core.Entities;
using DotNetBrightener.WebApi.GenericCRUD.Contracts;

namespace CRUDWebApiWithGeneratorDemo.Services;

public class CRUDDataServiceGeneratorRegistration : ICRUDDataServiceGeneratorRegistration
{
    public List<Type> Entities { get; } =
    [
		// provide all entities that need to generate CRUD data service for
        typeof(Product),
        typeof(ProductCategory)
    ];
}
```

The class must implement the `ICRUDDataServiceGeneratorRegistration` interface as the generator library relies on this contract to discern what needs generating.
 
In the `Entities` property of this class, ensure to include all entities for which CRUD data service interfaces and classes are required.
 
## Configure Code Generator for WebAPI Project
 
### Define auto-generate entities registration class
 
At the root of WebAPI project, create a class `CRUDWebApiGeneratorRegistration.cs` with the following content

```csharp
using CRUDWebApiWithGeneratorDemo.Core.Entities;
using CRUDWebApiWithGeneratorDemo.Services;
using DotNetBrightener.WebApi.GenericCRUD.Contracts;

public class CRUDWebApiGeneratorRegistration : ICRUDWebApiGeneratorRegistration
{
	// reference the CRUDDataServiceGeneratorRegistration from Service project
    public Type DataServiceRegistrationType { get; } = typeof(CRUDDataServiceGeneratorRegistration);

    public List<Type> Entities { get; } =
    [
		// provide all entities that need to generate CRUD API Controllers for
		// Some entities related to Authorization / Authentication / Security should be ignored
        typeof(Product),
        typeof(ProductCategory)
    ];
}
```

The class must implement the `ICRUDWebApiGeneratorRegistration` interface as the generator library relies on this contract to discern what needs generating.
 
In the `Entities` property of this class, ensure to include all entities for which CRUD Web API Controllers are required.
 
### Other configurations for Web API Project
 
In `Program.cs` or `Startup.cs`, you'll need to add the registration for the generated DataService interfaces into the ServiceCollection or Dependencies Container of your choice.
 
```csharp
// register the generated data services
builder.Services.AddScoped<IProductCategoryDataService, ProductCategoryDataService>();
builder.Services.AddScoped<IProductDataService, ProductDataService>();
// other service registration
```
One good approach is to implement a mechanism in your application to automatically detect the DataService classes in your `Service` project and register them automatically to the `ServiceCollection` or your dependencies container.
 
# Database Migration
 
This code generator instruction does not cover how to implement database migration. You will need to follow instruction from Microsoft Entity Framework or the ORM library that you use for your project.
 
# Build and Run Project
 
As you followed the instruction so far, upon building and running the project, the needed classes will be automatically generated. By default, .NET WebAPI project includes the OpenAPI library, which will detect the available APIs and you can see the following when the project is launch:
 
![[swagger_screenshot.png]]
 
# Auto-Generated Web APIs
 
The generator will generate the following APIs for each entity type registered in the registration.
 
### `GET` /api/{entity}
* Retrieve list of records of the `entity` type, and satisfies the filter, if provided
 
This API accepts the following query string parameters:
 
| Parameter | Type | Description |
| -- | -- | -- |
| columns | string[], separate by commas | The columns / fields of the entity to retrieve |
| pageSize | number | The number of records to return in one page, default is 20 |
| pageIndex | number | The index of the page to return, default is 0 |
| orderBy | string | The column to sort the result by, in ascending order. If the value starts with a hyphen (`-`) and followed by the column name, the result is sorted in descending order. This parameter impacts how the data is returned. |
| `{column_name}` | any | The filter expressions to filter the result by the `column_name`. Eg: `createdBy=user*` will filter the result to return the records that have `CreatedBy` value starts with `user`. Or, `summary=contains(value3)&createdDate=<=2023-12-01`, will filter the records that have `summary` value contains `value3` in the string, **and** `createdDate` is before `01 Dec 2023`.<br/><br/>*See the tables below for more detail.* | 
 
> `column_name` is case-sensitive. You will need to obtain the correct name of the column from the response body.
 
> There is no `OR` operator supported at the moment, because the nature of HTTP query strings combined by `&` operator. Therefore, only `AND` operator can be supported.
 
You can use custom queries for the `{column_name}` parameter with different operators. Here's a table explaining how to use them:
 
The follow operators are supported in most of all data types, `string`, `int`, `long`, `float`, `double`
 
| Operator | Symbol | Example Usage | Description|
|---|---|---|---|
| Equals | `eq` ***(default)***|`id=eq(100)` or `id=100`|Filters records where the `id` value matches the one on the right side of the query or in the parentheses.|
| Not Equals | `ne` |`id=ne_100` <br /> `id=ne(100)`<br/> `id=!(100)`|Filters the records where the `id` value doesn't not equal the value on the right operand or in the parentheses.|
| In | `in` | `summary=in(valuea,valueb)` | Filters the records where the `summary` is in the values defined in the parentheses.|
| Not In | `notin`<br /> `!in` | `summary=notin(valuea,valueb)`<br />`summary=!in(valuea,valueb)`  | Filters the records where the `summary` **IS NOT** in the values defined in the parentheses.|
 
The following operators are supported for `string` data type.
 
| Operator | Symbol | Example Usage | Description|
|---|---|---|---|
| Contains | `contains`<br/>`like`<br/>`*keyword*` | `summary=contains(value)`<br />`summary=like(value)`<br />`summary=*value*` | Filters the records where the `summary` contains the value in the parentheses.|
| Not Contains | `notcontains`<br/>`notlike`<br/>`!contains`<br />`!like` | `summary=!contains(value)`<br />`summary=!like(value)`| Filters the records where the `summary` **DOES NOT** contains the value in the parentheses.|
| Starts With | `startsWith`<br />`sw`<br/>`keyword*` | `summary=startsWith(value)`<br/>`summary=sw(value)`<br/>`summary=value*` | Filters the records where the `summary` starts with the value in the parentheses or on the right side of the query.|
| Not Starts With | `!startsWith`<br />`!sw` | `summary=!startsWith(value)`<br/>`summary=!sw(value)` | Filters the records where the `summary` **DOES NOT** start with the value in the parentheses.|
| Ends With | `endsWith`<br/>`ew`<br/>`*keyword` | `summary=endsWith(value)`<br />`summary=ew(value)`<br/>`summary=*value` | Filters the records where the `summary` ends with the value in the parentheses or on the right side of the query.|
| Not Ends With | `!endsWith`<br/>`!ew` | `summary=!endsWith(value)`<br />`summary=!ew(value)` | Filters the records where the `summary` **DOES NOT** end with the value in the parentheses.|
 
The following operators are supported for `int`, `float`, `double`, `decimal`, `datetime` and `datetimeoffset` data type.
 
| Operator | Symbol | Example Usage | Description|
|---|---|---|---|
|Greater Than<br />After (for `datetime` types)|`>`|`id=>(10)`<br />`displayIndex=>(200)`<br />`expirationDate=>(2023-12-01)`|-|
|Greater Than or Equals<br />On or After (for `datetime` type)|`>=`|`id=>=(10)`<br />`displayIndex=>=(200)`<br />`expirationDate=>=(2023-12-01)`|-|
|Less Than<br />Before (for `datetime` types)|`<`|`id=<(10)`<br />`displayIndex=<(200)`<br />`invoiceDate=<(2023-12-01)`|-|
|Less Than or Equals<br />Before or On (for `datetime` types)|`<=`|`id=<=(10)`<br />`displayIndex=<=(200)`<br />`invoiceDate=<=(2023-12-01)`|-|
 
### `GET` /api/{entity}/{id}
- Get the record of the `entity` type by `id`
 
This API accepts the following query string parameters
 
| Parameter | Type | Description |
| -- | -- | -- |
| columns | string[], separate by commas | The columns / fields of the entity to retrieve |
 
### `POST` /api/{entity}
* Create a new record of the `entity` type
 
### `PUT` /api/{entity}/{id}
* Update entity by `id`
 
The body of the request can be part of the entity. Only the provided fields in the body will be updated to the entity specified by the `id`
 
### `DELETE` /api/{entity}/{id}
* Delete entity by `id`
 
### `PUT` /api/{entity}/{id}/undelete
* Restore the **deleted** record of the `entity` by `id`
