using CRUDWebApiWithGeneratorDemo.gRPC.GrpcServices;
using CRUDWebApiWithGeneratorDemo.gRPC.Services;
using Bogus;
using CRUDWebApiWithGeneratorDemo.Core.Entities;
using CRUDWebApiWithGeneratorDemo.Database.DbContexts;
using CRUDWebApiWithGeneratorDemo.gRPC.Extensions;
using CRUDWebApiWithGeneratorDemo.Services.Data;
using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.EF.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.UseNLogLogging();
builder.Services.AddLogging();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// register the generated data services
builder.Services.AddScoped<IProductCategoryDataService, ProductCategoryDataService>();
builder.Services.AddScoped<IProductDataService, ProductDataService>();
builder.Services.AddScoped<IProductDocumentDataService, ProductDocumentDataService>();

builder.Services.AddControllers();

builder.Services.AddGrpc().AddJsonTranscoding();

var dbConfiguration = new DatabaseConfiguration
{
    ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection"),
    UseLazyLoading   = true,
    DatabaseProvider = DatabaseProvider.PostgreSql
};

Action<DbContextOptionsBuilder> configureDatabase = optionsBuilder =>
{
    optionsBuilder.UseSqlServer(dbConfiguration.ConnectionString);
};

builder.Services
       .AddEntityFrameworkDataServices<MainAppDbContext>(dbConfiguration,
                                                         configureDatabase);

var assemblies = AppDomain.CurrentDomain
                          .GetAssemblies();

builder.Services.AutoRegisterDependencyServices(assemblies);
var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

// Including Routing for RestAPI support. Added by DotNet Brightener gRPC Generator
app.UseRouting();

// Including Grpc Web Support. Added by DotNet Brightener gRPC Generator
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

#region gRPC service auto-registration
/****************************************************
 -----------------------------------------------------------------------
|          DotNet Brightener gRPC Service Generator Tool                |
|                               ---o0o---                               |
 -----------------------------------------------------------------------

This section of the file is generated by an automation tool, and it could 
be re-generated every time you build the project.

Don't change this section as your changes will be messed up if the section gets re-generated.

© 2024 DotNet Brightener. <admin@dotnetbrightener.com>

****************************************************/


/**   Auto registration of gRPC services will be here. DO NOT remove this comment   **/
#endregion gRPC service auto-registration



using (var scope = app.Services.CreateScope())
{
    app.Logger.LogInformation("Migrating database schema...");
    var dbContext = scope.ServiceProvider.GetRequiredService<MainAppDbContext>();

    dbContext.AutoMigrateDbSchema();
    app.Logger.LogInformation("Done migrating database schema...");
}

using (var scope = app.Services.CreateScope())
{
    app.Logger.LogInformation("Seeding database data...");
    var productService = scope.ServiceProvider.GetRequiredService<IProductDataService>();
    var productCategoryService = scope.ServiceProvider.GetRequiredService<IProductCategoryDataService>();
    var productDocumentDataService = scope.ServiceProvider.GetRequiredService<IProductDocumentDataService>();

    if (!productCategoryService.Fetch().Any())
    {
        var faker = new Faker();

        var categoriesList = new List<ProductCategory>();

        var fakeCategories = faker.Commerce.Categories(30)
                                  .Distinct()
                                  .ToArray();

        for (var i = 0; i < fakeCategories.Count(); i++)
        {
            var productCategory = new ProductCategory
            {
                Name = fakeCategories[i]
            };

            categoriesList.Add(productCategory);
        }

        productCategoryService.Insert(categoriesList);
    }

    if (!productService.Fetch().Any())
    {
        var faker = new Faker();

        var productsList = new List<Product>();

        for (var i = 0; i < 512; i++)
        {
            var product = new Product
            {
                Name        = faker.Commerce.ProductName(),
                Description = faker.Commerce.ProductDescription(),
            };

            productsList.Add(product);
        }

        productService.Insert(productsList);
    }

    if (!productDocumentDataService.Fetch().Any())
    {
        var faker = new Faker();

        var productsList = new List<ProductDocument>();

        for (var i = 0; i < 512; i++)
        {
            var product = new ProductDocument()
            {
                FileName     = faker.System.CommonFileName(),
                Description  = faker.Company.Bs(),
                FileUrl      = faker.System.DirectoryPath(),
                FileGuid     = Guid.NewGuid(),
                DisplayOrder = new Random().Next(1, 1000),
                Price        = decimal.Parse(faker.Commerce.Price())
            };

            productsList.Add(product);
        }

        productDocumentDataService.Insert(productsList);
    }
}


app.MapControllers();

app.Run();
