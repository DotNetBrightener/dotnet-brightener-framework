using DotNetBrightener.DataAccess;
using Microsoft.EntityFrameworkCore;
using MultiTenancy.Demo.WebApi.DbContexts;

var builder = WebApplication.CreateBuilder(args);
ExtendedServiceFactory.ApplyServiceProviderFactory(builder.Host);

var dbConfiguration = new DatabaseConfiguration
{
    ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection"),
    UseLazyLoading   = true,
    DatabaseProvider = DatabaseProvider.PostgreSql
};

Action<DbContextOptionsBuilder> configureDatabase = optionsBuilder =>
{
    optionsBuilder.UseNpgsql(dbConfiguration.ConnectionString);
};

builder.Services
       .AddEFCentralizedDataServices<MultiTenancyDbContext>(dbConfiguration,
                                                            builder.Configuration,
                                                            configureDatabase);

builder.Services.AddAutoMigrationForDbContextAtStartup<MultiTenancyDbContext>();

var multiTenantConfig = builder.Services
                               .EnableMultiTenancy<Clinic>();

multiTenantConfig.RegisterTenantMappableType<User>();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.EnableLazyResolver();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseTenantDetection<Clinic>();

app.MapControllers();

app.Run();
