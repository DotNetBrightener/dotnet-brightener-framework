# Data Migration Library for .NET Core Applications

&copy; 2025 [DotNet Brightener](mailto:admin@dotnetbrightener.com)

## Versions

| Package | Version |
| --- | --- |
| DotNetBrightener.DataAccess.DataMigration | ![NuGet Version](https://img.shields.io/nuget/v/DotNetBrightener.DataAccess.DataMigration) |
| DotNetBrightener.DataAccess.DataMigration.Mssql | ![NuGet Version](https://img.shields.io/nuget/v/DotNetBrightener.DataAccess.DataMigration.Mssql)|
| DotNetBrightener.DataAccess.DataMigration.PostgreSql | ![NuGet Version](https://img.shields.io/nuget/v/DotNetBrightener.DataAccess.DataMigration.PostgreSql)|
| dotnet-dnb-datamigration | ![NuGet Version](https://img.shields.io/nuget/v/dotnet-dnb-datamigration) |

## Overview

Data Migration Library is a simple library to help you manage your data migration in your .NET Core application. It provides a simple way to define your migration classes and run them in your application.

Not all applications can use DACPAC for managing database schema and data changes. Especially DACPAC does not work with other databases such as MySQL, PostgreSQL, etc. This library is designed to help you manage your data migration in your application.

## Installation

### Install using Package Reference
   
```bash
dotnet add [YOUR_PROJECT_NAME] package DotNetBrightener.DataAccess.DataMigration
```

If you need to data migration with SQL Server, install the following package: [`DotNetBrightener.DataAccess.DataMigration.Mssql`](https://www.nuget.org/packages/DotNetBrightener.DataAccess.DataMigration.Mssql)

```bash
dotnet add [YOUR_PROJECT_NAME] package DotNetBrightener.DataAccess.DataMigration.Mssql
```

For PostgreSQL, install the following package: [`DotNetBrightener.DataAccess.DataMigration.PostgreSql`](https://www.nuget.org/packages/DotNetBrightener.DataAccess.DataMigration.PostgreSql)

```bash
dotnet add [YOUR_PROJECT_NAME] package DotNetBrightener.DataAccess.DataMigration.PostgreSql
```

### Usage

#### Register to Service Collection

```csharp
// var _connectionString = "<your_connection_string>";

// If you use SQL Server
services.EnableDataMigrations()
        .UseSqlServer(_connectionString);

// If you use PostgreSQL
services.EnableDataMigrations()
        .UseNpgsql(_connectionString);

// if you want to auto detect all migration classes
services.AutoScanDataMigrators();

// if you want to manually register migration classes
services.AddDataMigrator<MyMigration>();

```

#### Define your migration classes

```csharp

using DotNetBrightener.DataAccess.DataMigration;

[DataMigration("<your_migration_id>")]
public class MyMigration : IDataMigration
{
    // You can use Dependency Injection for injecting your services
    private readonly IMyService _myService;

    public MyMigration(IMyService myService)
    {
        _myService = myService;
    }

    public async Task MigrateData()
    {
        // Your migration code here
        await _myService.DoSomethingToMigrateData();
    }
}

```

### Roadmap

- [x] Initial Release
- [x] Add Support for SQL Server
- [x] Add Support for PostgreSQL
- [x] CLI tool for creating migration class

### CLI Tool

You can now install the cli tool from Nuget: [dotnet-dnb-datamigration](https://www.nuget.org/packages/dotnet-dnb-datamigration)