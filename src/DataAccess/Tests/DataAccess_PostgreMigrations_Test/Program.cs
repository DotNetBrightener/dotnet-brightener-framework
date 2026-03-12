using DataAccess_PostgreMigrations_Test.Db.DbContexts;
using DataAccess_PostgreMigrations_Test.Migrations;
using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);
ExtendedServiceFactory.ApplyServiceProviderFactory(builder.Host);

// Add services to the container.


var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString");

var dbConfiguration = new DatabaseConfiguration
{
    ConnectionString = connectionString,
    DatabaseProvider = DatabaseProvider.PostgreSql
};

builder.Services.AddEFCentralizedDataServices<MainDbContext>(dbConfiguration,
                                                             builder.Configuration,
                                                             (serviceProvider, options) =>
                                                             {
                                                                 options.UseNpgsql(dbConfiguration
                                                                                      .ConnectionString,
                                                                                   s => s.EnableRetryOnFailure(10));
                                                             })
       .UseCentralizedMigrationDbContext<MigrationDbContext>();

builder.Services.EnableGuidV7ForPostgreSql<MainDbContext>();
builder.Services.EnableGuidV7ForPostgreSql<MigrationDbContext>();


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

    await using (repository.BeginUnitOfWork())
    {
        await repository.InsertAsync(new TestEntityWithAuditInfo());
    }
}

app.Run();
