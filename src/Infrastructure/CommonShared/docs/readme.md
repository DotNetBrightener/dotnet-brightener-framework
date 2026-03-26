# WebApp.CommonShared

A foundational infrastructure library providing reusable components, utilities, and patterns for ASP.NET Core web applications built on the DotNetBrightener framework.

## Installation

```bash
dotnet add package WebApp.CommonShared
```

## Features

- **Modular Endpoint System** - Organize API endpoints with route grouping, validation, and OpenAPI support
- **Automatic Dependency Registration** - Auto-discover and register services by interface
- **Async Task Processing** - Background task execution with progress tracking
- **Content Formatting** - Accept header-based content negotiation
- **Model Validation** - FluentValidation integration with validated route extensions
- **Result Type Handling** - Convert `Result<TValue, TError>` to HTTP responses
- **MVC Enhancements** - ETag caching, comma-separated array binding

---

## Table of Contents

1. [Endpoint Module System](#endpoint-module-system)
2. [Dependency Registration](#dependency-registration)
3. [Async Task Processing](#async-task-processing)
4. [Content Formatting](#content-formatting)
5. [Model Validation](#model-validation)
6. [Result Type Extensions](#result-type-extensions)
7. [MVC Enhancements](#mvc-enhancements)
8. [Exceptions](#exceptions)
9. [Services](#services)

---

## Endpoint Module System

### Overview

The endpoint module system provides a structured way to organize Minimal API endpoints with route grouping, cross-cutting concerns, and OpenAPI documentation support.

### Creating an Endpoint Module

```csharp
using WebApp.CommonShared.Endpoints;
using Microsoft.AspNetCore.Routing;

public class UsersModule : EndpointModuleBase
{
    protected override string BasePath => "/api/users";
    protected override string[] Tags => new[] { "Users" };

    protected override Action<RouteGroupBuilder>? ConfigureGroup => group =>
    {
        group.RequireAuthorization();
        group.RequireCors("DefaultPolicy");
    };

    protected override void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/", GetAllUsers)
            .WithOpenApiInfo("GetAllUsers", "Gets all users");

        app.MapPostWithValidation<CreateUserRequest>("/", CreateUser)
            .ProducesResponse<UserDto>(201)
            .ProducesError(400);
    }

    private static IResult GetAllUsers(IUserService userService)
        => Results.Ok(userService.GetAll());

    private static async Task<IResult> CreateUser(
        CreateUserRequest request,
        IUserService userService)
    {
        var user = await userService.CreateAsync(request);
        return Results.Created($"/api/users/{user.Id}", user);
    }
}
```

### Registration

```csharp
// In Program.cs
var assemblies = AppDomain.CurrentDomain.GetAssemblies();

// Register endpoint modules
builder.Services.AddEndpointModules(options =>
{
    options.Assemblies = assemblies;
    options.AutoRegisterValidators = true; // Enable FluentValidation
});

// Map endpoints
app.MapEndpointModules();
```

### EndpointModuleBase Properties

| Property | Type | Description |
|----------|------|-------------|
| `BasePath` | `string` | Base path for all routes in module (e.g., `/api/users`) |
| `Tags` | `string[]` | OpenAPI tags for grouping endpoints |
| `ConfigureGroup` | `Action<RouteGroupBuilder>?` | Configure auth, CORS, rate limiting, etc. |

### Route Extensions

```csharp
// Validated routes with automatic model validation
app.MapPostWithValidation<TRequest>("/users", handler);
app.MapPutWithValidation<TRequest>("/users/{id}", handler);
app.MapPatchWithValidation<TRequest>("/users/{id}", handler);

// Standard routes
app.MapGet("/users", handler);
app.MapPost("/users", handler);
app.MapPut("/users/{id}", handler);
app.MapDelete("/users/{id}", handler);
```

### OpenAPI Metadata Extensions

```csharp
app.MapGet("/users/{id}", GetUser)
    .WithOpenApiInfo("GetUser", "Gets a user by ID", "Returns user details")
    .WithEndpointTags("Users", "Administration")
    .ProducesResponse<UserDto>(200)
    .ProducesError(404)
    .WithCommonErrorResponses(includeNotFound: true);
```

---

## Dependency Registration

### Dependency Interfaces

```csharp
// Mark services for automatic registration
public interface IDependency { }

// Lifetime-specific interfaces
public interface ISingletonDependency : IDependency { }
public interface ITransientDependency : IDependency { }
// Default: IScopedDependency (implied by IDependency)
```

### Usage

```csharp
// Service implementation
public class EmailService : IEmailService, ISingletonDependency
{
    // Registered as Singleton
}

public class UserRepository : IUserRepository, IDependency
{
    // Registered as Scoped (default)
}

// Auto-registration in Program.cs
builder.Services.AutoRegisterDependencyServices(assemblies);
```

### Action Filter Providers

```csharp
public interface IActionFilterProvider
{
    IEnumerable<IFilterProvider> GetFilterProviders();
}

// Register custom filter provider
builder.Services.RegisterFilterProvider<MyFilterProvider>();
```

---

## Async Task Processing

### Overview

Run background tasks with progress tracking, status polling, and result retrieval.

### Creating an Async Task

```csharp
using WebApp.CommonShared.AsyncTasks;

public class ReportGenerationTask : IAsyncTask<ReportGenerationInput>
{
    private readonly IReportService _reportService;

    public ReportGenerationTask(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task ExecuteAsync(
        ReportGenerationInput input,
        AsyncTaskContext context,
        CancellationToken cancellationToken)
    {
        var totalRecords = await _reportService.GetCountAsync();

        for (int i = 0; i < totalRecords; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Process record...

            // Update progress
            context.UpdateProgress(
                processedRecords: i + 1,
                totalRecords: totalRecords
            );
        }

        // Set final result
        context.SetResult(new ReportGenerationResult { FilePath = "..." });
    }
}
```

### AsyncTaskContext Properties

| Property | Type | Description |
|----------|------|-------------|
| `TaskId` | `string` | Unique task identifier |
| `Status` | `AsyncTaskStatus` | Current status (Pending, Running, Completed, Failed) |
| `Progress` | `int` | Completion percentage (0-100) |
| `ProcessedRecords` | `long` | Number of records processed |
| `TotalRecords` | `long` | Total records to process |
| `Result` | `object?` | Task result data |
| `Error` | `string?` | Error message if failed |

### API Controller

```csharp
// Extend AsyncTaskApiControllerBase for built-in endpoints
[ApiController]
[Route("api/async-tasks")]
public class AsyncTaskController : AsyncTaskApiControllerBase
{
    public AsyncTaskController(IAsyncTaskScheduler scheduler)
        : base(scheduler)
    {
    }
}

// Endpoints provided:
// POST   /api/async-tasks/{taskId}/start    - Start task
// GET    /api/async-tasks/{taskId}/status   - Get status
// GET    /api/async-tasks/{taskId}/result   - Get result
// DELETE /api/async-tasks/{taskId}          - Cancel/delete
```

---

## Content Formatting

### Overview

Content negotiation based on Accept headers with extensible formatters.

### IContentFormatter Interface

```csharp
public interface IContentFormatter
{
    IEnumerable<string> SupportedContentTypes { get; }
    bool CanFormat(string contentType);
    Task FormatAsync<T>(HttpContext context, T? value, CancellationToken ct = default);
}
```

### Registration

```csharp
// Register formatters
builder.Services.AddContentFormatters(options =>
{
    // JSON formatter registered by default
    options.AddFormatter<XmlContentFormatter>();
    options.AddFormatter<CsvContentFormatter>();
});
```

### Usage in Endpoints

```csharp
app.MapGet("/api/data", async (HttpContext context, IDataService service) =>
{
    var data = await service.GetDataAsync();
    await context.FormatResponse(data);
});
```

### Accept Header Extensions

```csharp
// Check if client accepts a content type
if (request.AcceptsContentType("application/json"))
{
    // Return JSON
}

// Get ordered list of accepted content types
var acceptedTypes = request.GetAcceptedContentTypes();
// Returns: [{ MediaType: "application/json", Quality: 1.0 }, ...]

// Get best matching content type
var bestMatch = request.GetBestContentType("application/json", "application/xml");
```

---

## Model Validation

### FluentValidation Integration

```csharp
// Define validator
public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}
```

### Manual Validation

```csharp
app.MapPost("/users", async (CreateUserRequest request, HttpContext context) =>
{
    // Validate manually
    var validationResult = context.Validate(request);
    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    // Process valid request
    return Results.Ok();
});
```

### Validation Extensions

```csharp
// Validate and get result in one call
var validation = context.ValidateAndGetResult(request);
if (validation != null) return validation; // Returns 400 with errors

// Async validation
var validationResult = await context.ValidateAsync(request, cancellationToken);
```

### Validated Route Extensions

```csharp
// Automatic validation before handler
app.MapPostWithValidation<CreateUserRequest>("/users",
    async (CreateUserRequest request, IUserService service) =>
    {
        // request is already validated
        var user = await service.CreateAsync(request);
        return Results.Created($"/users/{user.Id}", user);
    });
```

---

## Result Type Extensions

### Converting Result<TValue, TError> to HTTP Responses

```csharp
using WebApp.CommonShared.Extensions;

// In controllers
[HttpGet("{id}")]
public IActionResult GetUser(int id)
{
    var result = _userService.GetById(id);

    return result.ToActionResult(
        onSuccess: user => Ok(user),
        onError: error => NotFound(error)
    );
}

// In minimal APIs
app.MapGet("/users/{id}", (int id, IUserService service) =>
{
    var result = service.GetById(id);
    return result.ToResult(
        successStatusCode: 200,
        errorStatusCode: 404
    );
});
```

---

## MVC Enhancements

### ETag Caching

```csharp
[HttpGet("{id}")]
[ETagFilter] // Automatic ETag generation and 304 responses
public IActionResult GetUser(int id)
{
    var user = _userService.GetById(id);
    return Ok(user);
}
```

### Comma-Separated Array Binding

```csharp
// GET /api/users?ids=1,2,3,4
[HttpGet]
public IActionResult GetUsers([FromQuery] int[] ids)
{
    // ids = [1, 2, 3, 4]
    return Ok(_userService.GetByIds(ids));
}

// Supports: int, long, short, byte, uint, ulong, ushort, Guid
```

---

## Exceptions

### NotFoundError

```csharp
// General 404 error
throw new NotFoundError("Resource not found");

// Type-specific 404 error
public class UserNotFoundError : ObjectNotFoundBaseProblemDetailsError<int>
{
    public UserNotFoundError(int userId)
        : base(userId, "User", $"User with ID {userId} not found")
    {
    }
}
```

---

## Services

### ITimezoneHandler

```csharp
public interface ITimezoneHandler
{
    string ConvertIanaToWindows(string ianaTimezoneId);
    string ConvertWindowsToIana(string windowsTimezoneId);
}

// Usage
var handler = serviceProvider.GetRequiredService<ITimezoneHandler>();
var windowsTz = handler.ConvertIanaToWindows("Asia/Ho_Chi_Minh");
// Returns: "SE Asia Standard Time"
```

---

## Common Web App Services

### Service Registration

```csharp
// Register all common services
var commonAppBuilder = builder.Services.AddCommonWebAppServices(builder.Configuration);

// Configure options via returned builder
commonAppBuilder.EventPubSubServiceBuilder.AddEventHandler<MyEventHandler>();

// Register MVC services
builder.Services.AddCommonMvcApp(builder.Configuration);
```

### Middleware Configuration

```csharp
var app = builder.Build();

// Apply common middleware
app.UseCommonWebAppServices();

// Maps endpoints from registrars
app.MapEndpointsFromRegistrars();

app.Run();
```

### What AddCommonWebAppServices Registers

- Forwarded Headers middleware
- HTTPS Redirection (non-container only)
- CORS (if configured)
- Problem Details
- HttpContextAccessor
- Caching services
- Crypto engine
- Background tasks
- Event Pub/Sub
- Permission authorization
- JSON options configuration

---

## Configuration

### CORS Configuration

```json
{
  "AllowedCorsOrigins": "https://localhost:3000,https://example.com"
}
```

### Environment Variables

| Variable | Description |
|----------|-------------|
| `ASPNETCORE__AllowedCorsOrigins` | Comma-separated CORS origins |
| `DOTNET_RUNNING_IN_CONTAINER` | Skip HTTPS redirection when true |

---

## Dependencies

This package depends on:

- `FluentValidation.DependencyInjectionExtensions`
- `Microsoft.AspNetCore.Mvc.NewtonsoftJson`
- `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation`
- `DotNetBrightener.Caching.Memory`
- `DotNetBrightener.CryptoEngine.DependencyInjection`
- `DotNetBrightener.Core.BackgroundTasks`
- `DotNetBrightener.Plugins.EventPubSub.DependencyInjection`
- `DotNetBrightener.Infrastructure.Security`
- `DotNetBrightener.Infrastructure.JwtAuthentication`

---

## License

MIT License - See [LICENSE](../../LICENSE) for details.
