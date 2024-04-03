using AspNet.Extensions.SelfDocumentedProblemResult.ExceptionHandlers;
using DotNetBrightener.DataAccess;
using Microsoft.EntityFrameworkCore;
using WebAppCommonShared.Demo.DbContexts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Host.UseNLogLogging();

builder.Services.ConfigureLogging(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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


builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddExceptionHandler<UnhandledExceptionResponseHandler>();

builder.Services.AddProblemDetails();

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

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MainAppDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
