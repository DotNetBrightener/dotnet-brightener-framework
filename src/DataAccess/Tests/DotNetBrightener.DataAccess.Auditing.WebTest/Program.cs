using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.Auditing.WebTest.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);
ExtendedServiceFactory.ApplyServiceProviderFactory(builder.Host);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new
        Exception("No connection string configured. Please add 'DefaultConnection' connection string with valid value to the Environment or appsettings.json file.");
}

builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddLocalization();
builder.Services.AddSwaggerGen();

var dbConfiguration = new DatabaseConfiguration
{
    ConnectionString = connectionString,
    UseLazyLoading = true,
    DatabaseProvider = DatabaseProvider.MsSql
};

Action<IServiceProvider, DbContextOptionsBuilder> configureDatabase = (serviceProvider, optionsBuilder) =>
{
    optionsBuilder.UseSqlServer(dbConfiguration.ConnectionString);
};

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddProblemDetails();

builder.Services.AddEventPubSubService();


builder.Services
       .AddEFCentralizedDataServices<MainAppDbContext>(dbConfiguration,
                                                       builder.Configuration,
                                                       configureDatabase);
builder.Services
       .UseMigrationDbContext<MainAppDbContext>(configureDatabase);

builder.Services.AddAuditingSqlServerStorage(dbConfiguration.ConnectionString);

var assemblies = AppDomain.CurrentDomain
                          .GetAppOnlyAssemblies();

builder.Services.AddEventHandlersFromAssemblies(assemblies);

var app = builder.Build();

app.UseForwardedHeaders();

if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
{
    app.UseHttpsRedirection();
}

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/testAudit",
           async (string           name,
                  MainAppDbContext dbContext) =>
           {
               await dbContext.AddAsync(new TestEntity
               {
                   EntityName = name
               });

               await dbContext.SaveChangesAsync();

               return Results.Ok();
           });

app.MapGet("/api/testAuditUpdate/{id}",
           async (long id,
                  string           name,
                  MainAppDbContext dbContext) =>
           {
               TestEntity updateEntity = dbContext.Set<TestEntity>()
                                                  .Find(id);


               if (updateEntity is null)
                   return Results.NotFound();

               updateEntity.EntityName = name;

               dbContext.Update(updateEntity);

               await dbContext.SaveChangesAsync();

               return Results.Ok();
           });


app.MapGet("/api/testAuditDelete/{id}",
           async (long id,
                  MainAppDbContext dbContext) =>
           {
               TestEntity updateEntity = dbContext.Set<TestEntity>()
                                                  .Find(id);

               if (updateEntity is null)
                   return Results.NotFound();

               dbContext.Remove(updateEntity);

               await dbContext.SaveChangesAsync();

               return Results.Ok();
           });

app.Run();
