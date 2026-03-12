# DotNetBrightener SiteSettings Module

## Overview

The DotNetBrightener SiteSettings module provides a comprehensive solution for managing application-wide configuration settings in .NET applications. It offers a flexible, type-safe approach to storing and retrieving settings with built-in caching, database persistence, and RESTful API endpoints.

### Key Features

- **Type-safe settings management** with strongly-typed setting classes
- **Database persistence** with support for SQL Server and PostgreSQL
- **Built-in caching** for optimal performance
- **RESTful API endpoints** for CRUD operations
- **Automatic database migrations** and schema management
- **Localization support** for error messages and descriptions
- **Audit trail** with creation/modification tracking
- **Default value merging** for backward compatibility

## Architecture

The module follows a layered architecture:

- **Controllers**: RESTful API endpoints (`SiteSettingsController`)
- **Services**: Business logic (`ISiteSettingService`, `SiteSettingService`)
- **Data Access**: Repository pattern (`ISiteSettingDataService`)
- **Models**: Setting definitions (`SiteSettingBase`, `SiteSettingWrapper<T>`)
- **Storage**: Database-specific implementations (SQL Server, PostgreSQL)

## Required Dependencies

### Core NuGet Packages

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.7" />
<PackageReference Include="Microsoft.Extensions.Localization" Version="9.0.7" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
```

### Database-Specific Packages

**For SQL Server:**
```xml
<PackageReference Include="DotNetBrightener.SiteSettings.Data.Mssql" Version="[latest]" />
```

**For PostgreSQL:**
```xml
<PackageReference Include="DotNetBrightener.SiteSettings.Data.PostgreSql" Version="[latest]" />
```

### Framework Dependencies

- **DotNetBrightener.Caching.Abstractions**: For caching functionality
- **DotNetBrightener.DataAccess.Abstractions**: For data access patterns

## Integration Steps

### Step 1: Install Required Packages

```bash
# Core package
dotnet add package DotNetBrightener.SiteSettings

# Choose your database provider
dotnet add package DotNetBrightener.SiteSettings.Data.Mssql
# OR
dotnet add package DotNetBrightener.SiteSettings.Data.PostgreSql
```

### Step 2: Define Your Settings Classes

Create strongly-typed setting classes by inheriting from `SiteSettingWrapper<T>`:

```csharp
using DotNetBrightener.SiteSettings.Models;

public class ApplicationSettings : SiteSettingWrapper<ApplicationSettings>
{
    public string ApplicationName { get; set; } = "My Application";
    public string SupportEmail { get; set; } = "support@example.com";
    public int MaxFileUploadSize { get; set; } = 10485760; // 10MB
    public bool EnableNotifications { get; set; } = true;

    public override string SettingName => "Application Settings";
    public override string Description => "General application configuration settings";
}

public class EmailSettings : SiteSettingWrapper<EmailSettings>
{
    public string SmtpServer { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 587;
    public string Username { get; set; }
    public string Password { get; set; }
    public bool EnableSsl { get; set; } = true;

    public override string SettingName => "Email Settings";
    public override string Description => "SMTP configuration for email sending";
}
```

### Step 3: Configure Services in Program.cs

```csharp
using DotNetBrightener.SiteSettings;
using DotNetBrightener.SiteSettings.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add localization support
builder.Services.AddLocalization();

// Add MVC services
var mvcBuilder = builder.Services.AddControllersWithViews();

// Register SiteSettings API
mvcBuilder.RegisterSiteSettingApi();

// Configure database storage (choose one)
// For SQL Server:
builder.Services.AddSiteSettingsSqlServerStorage(
    builder.Configuration.GetConnectionString("DefaultConnection"));

// For PostgreSQL:
// builder.Services.AddSiteSettingsPostgreSqlStorage(
//     builder.Configuration.GetConnectionString("DefaultConnection"));

// Register your setting types
builder.Services.RegisterSettingType<ApplicationSettings>();
builder.Services.RegisterSettingType<EmailSettings>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapControllers();

app.Run();
```

### Step 4: Database Configuration

Add connection string to `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyApp;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

The module will automatically create the required database schema and tables on first run.

## API Endpoints

The module exposes the following RESTful endpoints:

### Get All Available Settings
```http
GET /api/SiteSettings/allSettings
```

**Response:**
```json
[
  {
    "settingName": "Application Settings",
    "description": "General application configuration settings",
    "settingType": "MyApp.ApplicationSettings"
  }
]
```

### Get Setting Values
```http
GET /api/SiteSettings/{settingKey}
```

**Example:**
```http
GET /api/SiteSettings/MyApp.ApplicationSettings
```

**Response:**
```json
{
  "applicationName": "My Application",
  "supportEmail": "support@example.com",
  "maxFileUploadSize": 10485760,
  "enableNotifications": true
}
```

### Get Default Setting Values
```http
GET /api/SiteSettings/{settingKey}/default
```

### Save Setting Values
```http
POST /api/SiteSettings/{settingKey}
Content-Type: application/json

{
  "applicationName": "Updated App Name",
  "supportEmail": "newsupport@example.com",
  "maxFileUploadSize": 20971520,
  "enableNotifications": false
}
```

## Programmatic Usage

### Inject and Use Settings Service

```csharp
using DotNetBrightener.SiteSettings.Abstractions;

[ApiController]
[Route("api/[controller]")]
public class MyController : ControllerBase
{
    private readonly ISiteSettingService _settingService;

    public MyController(ISiteSettingService settingService)
    {
        _settingService = settingService;
    }

    [HttpGet("app-info")]
    public IActionResult GetAppInfo()
    {
        var appSettings = _settingService.GetSetting<ApplicationSettings>();
        
        return Ok(new
        {
            Name = appSettings.ApplicationName,
            Support = appSettings.SupportEmail,
            MaxUpload = appSettings.MaxFileUploadSize
        });
    }

    [HttpPost("update-settings")]
    public IActionResult UpdateSettings([FromBody] ApplicationSettings settings)
    {
        _settingService.SaveSetting(settings);
        return Ok();
    }
}
```

## Configuration Options

### Caching Configuration

The module uses the DotNetBrightener caching system. Settings are cached for 10 minutes by default. Configure caching in your startup:

```csharp
// Add memory caching
builder.Services.AddMemoryCache();

// Or Redis caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});
```

### Database Schema

The module creates tables in the following schema:

**SQL Server:** `SiteSettings.SiteSettingRecord`
**PostgreSQL:** `site_settings.site_setting_record`

**Table Structure:**
- `Id` (bigint, primary key)
- `SettingType` (nvarchar(1024)) - Fully qualified type name
- `SettingContent` (nvarchar(max)) - JSON serialized settings
- Audit fields: `CreatedDate`, `CreatedBy`, `ModifiedDate`, `ModifiedBy`
- Soft delete fields: `IsDeleted`, `DeletedDate`, `DeletedBy`, `DeletionReason`

## Error Handling

The module provides localized error messages. Common error scenarios:

### Setting Type Not Found
```json
{
  "errorMessage": "Setting 'InvalidType' not found"
}
```

### Invalid Setting Type
```json
{
  "errorMessage": "Setting type must inherit from SiteSettingBase"
}
```

## Advanced Features

### Custom Contract Resolver

The module uses a custom JSON contract resolver (`SiteSettingsContractResolver`) for consistent serialization.

### Default Value Merging

When retrieving settings, the module automatically merges saved values with default values, ensuring backward compatibility when new properties are added to setting classes.

### Audit Trail

All setting changes are automatically tracked with creation and modification timestamps and user information.

## Testing

### Unit Testing Example

```csharp
[Test]
public void Should_Save_And_Retrieve_Settings()
{
    // Arrange
    var settings = new ApplicationSettings
    {
        ApplicationName = "Test App",
        SupportEmail = "test@example.com"
    };

    // Act
    _settingService.SaveSetting(settings);
    var retrieved = _settingService.GetSetting<ApplicationSettings>();

    // Assert
    Assert.AreEqual("Test App", retrieved.ApplicationName);
    Assert.AreEqual("test@example.com", retrieved.SupportEmail);
}
```

## Troubleshooting

### Common Issues

1. **Database Connection Issues**
   - Verify connection string format
   - Ensure database server is accessible
   - Check firewall settings

2. **Migration Issues**
   - Ensure proper permissions for schema creation
   - Check if migrations are enabled in configuration

3. **Caching Issues**
   - Verify caching service is properly registered
   - Check cache key conflicts

4. **Serialization Issues**
   - Ensure setting properties have public getters/setters
   - Avoid circular references in setting objects

### Performance Considerations

- Settings are cached for 10 minutes by default
- Use appropriate database indexes for `SettingType` column
- Consider using Redis for distributed caching in multi-instance deployments

## Migration from Other Configuration Systems

### From appsettings.json

1. Create setting classes for your configuration sections
2. Register setting types in DI container
3. Migrate values using the API or programmatically
4. Update code to use `ISiteSettingService` instead of `IConfiguration`

### From Custom Configuration Tables

1. Create migration scripts to transform existing data
2. Map existing columns to setting class properties
3. Use the `SaveSetting` method to populate the new system

This integration guide provides everything needed to successfully implement the DotNetBrightener SiteSettings module in your .NET applications.
