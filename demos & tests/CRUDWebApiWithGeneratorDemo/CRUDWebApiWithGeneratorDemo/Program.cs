using Bogus;
using CRUDWebApiWithGeneratorDemo.Core.Entities;
using CRUDWebApiWithGeneratorDemo.Database.DbContexts;
using CRUDWebApiWithGeneratorDemo.Services.Data;
using DotNetBrightener.DataAccess;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.UseNLogLogging();
builder.Services.AddLogging();
builder.Services.AddHttpContextAccessor();


// register the generated data services
builder.Services.AddScoped<IProductCategoryDataService, ProductCategoryDataService>();
builder.Services.AddScoped<IProductDataService, ProductDataService>();
builder.Services.AddScoped<IProductDocumentDataService, ProductDocumentDataService>();

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

builder.Services
       .AddEntityFrameworkDataServices<MainAppDbContext>(dbConfiguration,
                                                         builder.Configuration,
                                                         configureDatabase);

builder.Services.AddAutoMigrationForDbContextAtStartup<MainAppDbContext>();

builder.Services
       .AddControllers()
       .AddNewtonsoftJson(o =>
        {
            o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            o.SerializerSettings.ContractResolver      = new CamelCasePropertyNamesContractResolver();
        });

var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    app.Logger.LogInformation("Seeding database data...");
    var productService = scope.ServiceProvider.GetRequiredService<IProductDataService>();
    var productCategoryService = scope.ServiceProvider.GetRequiredService<IProductCategoryDataService>();
    var productDocumentDataService = scope.ServiceProvider.GetRequiredService<IProductDocumentDataService>();

    var categoriesList = new List<ProductCategory>();
    if (!productCategoryService.Fetch().Any())
    {
        var faker = new Faker();
        categoriesList.Clear();

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

        productCategoryService.InsertMany(categoriesList);
    }


    categoriesList = productCategoryService.Fetch().ToList();
    if (!productService.Fetch().Any())
    {
        var faker = new Faker();

        var productsList = new List<Product>();
        var random       = new Random();

        for (var i = 0; i < 512; i++)
        {
            var randomCategoryId = categoriesList.ElementAt(random.Next(1, categoriesList.Count)).Id;

            var product = new Product
            {
                Name              = faker.Commerce.ProductName(),
                Description       = faker.Commerce.ProductDescription(),
                ProductCategoryId = randomCategoryId
            };

            productsList.Add(product);
        }

        productService.InsertMany(productsList);
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

        productDocumentDataService.InsertMany(productsList);
    }
}

app.Run();
