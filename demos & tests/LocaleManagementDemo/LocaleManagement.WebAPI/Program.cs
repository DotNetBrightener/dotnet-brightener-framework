using DotNetBrightener.Caching.Memory;
using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.EF.Extensions;
using DotNetBrightener.LocaleManagement.WebApi;
using LocaleManagement.Database.DbContexts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


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
                                                         configureDatabase);

builder.Services
       .EnableCachingService()
       .EnableMemoryCacheService();

builder.Services.AddLocalization();

var mvcBuilder = builder.Services.AddControllers();

mvcBuilder.AddNewtonsoftJson(o =>
{
    var serializerSettings = DefaultJsonSerializer.DefaultJsonSerializerSettings;

    o.SerializerSettings.ContractResolver      = serializerSettings.ContractResolver;
    o.SerializerSettings.ReferenceLoopHandling = serializerSettings.ReferenceLoopHandling;
});

mvcBuilder.RegisterLocaleManagementApi(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    app.Logger.LogInformation("Migrating database schema...");
    var dbContext = scope.ServiceProvider.GetRequiredService<MainAppDbContext>();

    dbContext.AutoMigrateDbSchema();
    app.Logger.LogInformation("Done migrating database schema...");
}

app.Run();