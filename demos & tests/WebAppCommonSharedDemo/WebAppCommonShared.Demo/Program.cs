using AspNet.Extensions.SelfDocumentedProblemResult.ExceptionHandlers;
using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebAppCommonShared.Demo.DbContexts;
using WebAppCommonShared.Demo.Entities;
using WebAppCommonShared.Demo.StartupTasks;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddInfisicalSecretsProvider();

builder.Host.UseNLogLogging();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services
       .ConfigureLogging(builder.Configuration)
       .AddLogStorage(optionsBuilder =>
        {
            optionsBuilder.UseSqlServer(connectionString);
        });

builder.Services.AddControllers();
builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }); ;

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var dbConfiguration = new DatabaseConfiguration
{
    ConnectionString = connectionString,
    UseLazyLoading = true,
    DatabaseProvider = DatabaseProvider.MsSql
};

Action<DbContextOptionsBuilder> configureDatabase = optionsBuilder =>
{
    optionsBuilder.UseSqlServer(dbConfiguration.ConnectionString);
};

builder.Services.AddEntityFrameworkDataServices<MainDbContext>(dbConfiguration,
                                                               builder.Configuration,
                                                               configureDatabase);

builder.Services.UseMigrationDbContext<MainDbContext>();


builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddExceptionHandler<UnhandledExceptionResponseHandler>();

builder.Services.AddProblemDetails();

builder.Services.RegisterStartupTask<SeedDataStartupTask>();

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

app.UseAuthorization();

app.MapControllers();

app.MapClientTelemetryEndpoint("api/telemetry");

app.MapErrorDocsTrackerUI(options =>
{
    options.UiPath = "/errors";
    options.ApplicationName = "Test Error";
});

app.MapGet("/api/subscription",
           async (SubscriptionStatus status,
                  HttpContext context, 
                  IRepository repository) =>
           {
               var result =  repository.Fetch<Subscription>(_ => _.Status == status)
                                       .Skip(0)
                                       .Take(100);

               return Results.Ok(result);
           });

app.Run();
