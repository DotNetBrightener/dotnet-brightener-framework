# Centralized CRUD WebAPI in ASP.NET Core Applications
 
&copy; 2025 DotNet Brightener. <admin@dotnetbrightener.com>
 
# Instruction
 
Most applications rely on CRUD operations. This library provides core CRUD functionalities, exposing them as WebAPI controllers, utilizing [DotNetBrightener.DataAccess.Abstractions](https://www.nuget.org/packages/DotNetBrightener.DataAccess.Abstractions/) library, which facilitates the database access layer performing the CRUD operations. 

# Implement in your project

Let's say in your project, you have the following class defined:

```csharp
public class ProductDocument : BaseEntityWithAuditInfo
{
    [MaxLength(255)]
    public string FileName { get; set; }
 
    [MaxLength(1024)]
    public string Description { get; set; }
 
    [MaxLength(2048)]
    public string FileUrl { get; set; }
 
    public Guid? FileGuid { get; set; }
 
    public int? DisplayOrder { get; set; }
 
    [DataType(DataType.Currency)]
    public decimal? Price { get; set; }
}
```

You will need to create a DataService interface and implementation class like the following:

```csharp
public partial interface IProductDocumentDataService : IBaseDataService<ProductDocument> { }
 
public partial class ProductDocumentDataService : BaseDataService<ProductDocument>, IProductDocumentDataService {
    
    public ProductDocumentDataService(IRepository repository)
        : base(repository)
    {
    }
}
```

The `IBaseDataService<>` interface and `BaseDataService<>` base class are defined in [DotNetBrightener.DataAccess.Abstractions](https://www.nuget.org/packages/DotNetBrightener.DataAccess.Abstractions) library. They provided CRUD operation already so you will not need to write CRUD operation yourself. The implementation is highly customizable, so you can change the logic based on your application's need.
 
You can then create a controller as followed:

```csharp

[ApiController]
[Route("api/[controller]")]
public partial class ProductDocumentController : BaseCRUDController<ProductDocument>
{
    public ProductDocumentController(
            IProductDocumentDataService dataService,
            IHttpContextAccessor httpContextAccessor)
        : base(dataService, httpContextAccessor)
    {
    }
}
```

In `Startup.cs` or `Program.cs`, register your DataService interface and implementation class to the `ServiceCollection` as followed:

```csharp
builder.Services.AddScoped<IProductDocumentDataService, ProductDocumentDataService>();
```

If you use CORS, you will need to add the following to the `ConfigureServices` method in `Startup.cs`:

```csharp

    services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
                            builder =>
                            {
                                // other configurations for your CORS policy builder

                                // builder.AllowAnyMethod()
                                //        .AllowAnyHeader();

                                  
                                // This is required for the headers that returned from the paged list API to be exposed to the consumers
                                builder.AddPagedDataSetExposedHeaders();
                            });
    });

```

Now your API is available. Check out the next section for the available APIs and what to expect.
 
# Available CRUD APIs

The following API for CRUD will be available when you implement the CRUD controllers.
 
### `GET` /api/[entity]
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
|Greater Than<br />After (for `datetime` types)|`>` `gt`|`id=>(10)`<br />`displayIndex=>(200)`<br />`expirationDate=>(2023-12-01)`|-|
|Greater Than or Equals<br />On or After (for `datetime` type)|`>=` `ge`|`id=>=(10)`<br />`displayIndex=>=(200)`<br />`expirationDate=>=(2023-12-01)`|-|
|Less Than<br />Before (for `datetime` types)|`<` `lt`|`id=<(10)`<br />`displayIndex=<(200)`<br />`invoiceDate=<(2023-12-01)`<br />`invoiceDate=lt(2023-12-01)`|-|
|Less Than or Equals<br />Before or On (for `datetime` types)|`<=` `le`|`id=<=(10)`<br />`displayIndex=<=(200)`<br />`invoiceDate=<=(2023-12-01)`<br />`invoiceDate=le(2023-12-01)`|-|
 
 
The following operators are supported for `datetimeoffset` data type.
 
| Operator | Symbol | Example Usage | Description|
|---|---|---|---|
| ON |`on`|`expiredDate=on(2024-06-05T00:00:00.000+07:00)`| Retrieve records that have `expiredDate` occurs between (inclusively) `00:00:00` and `23:59:59` on the `5th of June, 2024` at timezone `+07:00` |
| NOT ON |`!on` `noton`|`expiredDate=noton(2024-06-05T00:00:00.000+07:00)`<br /><br />`expiredDate=!on(2024-06-05T00:00:00.000+07:00)`| Retrieve records that have `expiredDate` occurs not on the 5th of June, 2024 at timezone `+07:00`. It means the result includes records with `expiredDate` before `12:00AM June 5th`, and records with `expiredDate` after `11:59:59PM June 5th`.<br/>It results in same result as of the query `expiredDate=!in(2024-06-05T00:00:00.000+07:00,2024-06-05T23:59:59.000+07:00)` |
| IN |`in`|`expiredDate=in(2024-06-05T00:00:00.000+07:00,2024-06-07T12:30:00.000+07:00)`| Retrieve records that have `expiredDate` occurs between the `5th of June, 2024` and `12:30 PM 7th of June, 2024` at timezone `+07:00` |
| NOT IN |`!in` `notin`|`expiredDate=notin(2024-06-05T00:00:00.000+07:00,2024-06-07T00:00:00.000+07:00)`<br /><br />`expiredDate=!in(2024-06-05T00:00:00.000+07:00,2024-06-07T12:30:00.000+07:00)`| Retrieve records that have `expiredDate` occurs before the `5th of June, 2024` or after `12:30 PM 7th of June, 2024` at timezone `+07:00` |
 
The response of the API also has the headers as followed that help you identify the total items available, the result count, requested page size and requested page index. See the below table for details.

| Header | Description |
| --- | --- |
| `X-Total-Count`  | The total number of items available based on the filter defined in the request |
| `X-Result-Count`  | The number of items returned in the current page |
| `X-Page-Size` | The requested page size |
| `X-Page-Index` | The requested page index |

### `GET` /api/[entity]/[id]
- Get the record of the `entity` type by `id`
 
This API accepts the following query string parameters
 
| Parameter | Type | Description |
| -- | -- | -- |
| columns | string[], separate by commas | The columns / fields of the entity to retrieve |
 
### `POST` /api/[entity]
* Create a new record of the `entity` type
 
### `PUT` /api/[entity]/{id}
* Update entity by `id`
 
The body of the request can be part of the entity. Only the provided fields in the body will be updated to the entity specified by the `id`
 
### `DELETE` /api/[entity]/{id}
* Delete entity by `id`
 
### `PUT` /api/[entity]/{id}/undelete
* Restore the **deleted** record of the `entity` by `id`