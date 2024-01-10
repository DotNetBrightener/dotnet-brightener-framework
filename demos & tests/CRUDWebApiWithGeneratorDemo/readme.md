# This is demo project of how to use the generator library.

Follow the instruction from here: https://www.nuget.org/packages/DotNetBrightener.WebApi.GenericCRUD.Generator/

# Stuffs added to the project

### In `Program.cs` file

For demonstrating purpose, we need to add manually the generated service classes to the service collection.

In actual project, we will need to detect the generated service classes and add them to the service collection automatically.

```csharp

// register the generated data services
builder.Services.AddScoped<IProductCategoryDataService, ProductCategoryDataService>();
builder.Services.AddScoped<IProductDataService, ProductDataService>();
// other service registration

```

There is also section in the `Program.cs` that adds configuration for accessing database

```csharp

var dbConfiguration = new DatabaseConfiguration
{
    ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection"),
    UseLazyLoading   = true,
    DatabaseProvider = DatabaseProvider.MsSql
};

Action<DbContextOptionsBuilder> configureDatabase = optionsBuilder =>
{
    optionsBuilder.UseSqlServer(dbConfiguration.ConnectionString);
};

// this line tells the framework that we're going to use MainAppDbContext as the only DbContext in the system
builder.Services
       .AddEntityFrameworkDataServices<MainAppDbContext>(dbConfiguration,
                                                         configureDatabase);

```

### In Database project
1. Created `MainAppDbcontext.cs`, so that we can use the same dbcontext for all the data services.

2. Created class `DesignTimeAppDbContext` so that we can create migration from the database project.



### Create migration

Open terminal in database project and run the following command

```bash

dotnet ef migrations add InitializeDatabase 
```

> The only project that should reference Database project is the WebApi project.