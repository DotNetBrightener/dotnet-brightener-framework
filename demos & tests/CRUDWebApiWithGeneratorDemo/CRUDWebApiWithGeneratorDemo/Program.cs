using CRUDWebApiWithGeneratorDemo;
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
builder.Services.AddScoped<IGroupEntityDataService, GroupEntityDataService>();

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
       .AddEFCentralizedDataServices<MainAppDbContext>(dbConfiguration,
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

app.Run();
