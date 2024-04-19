using AspNet.Extensions.SelfDocumentedProblemResult.ExceptionHandlers;
using DotNetBrightener.DataAccess;
using DotNetBrightener.Infrastructure.JwtAuthentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json.Serialization;
using DotNetBrightener.WebSocketExt.Authentication;
using DotNetBrightener.WebSocketExt.Messages;
using DotNetBrightener.WebSocketExt.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebAppCommonShared.Demo.WebSocketCommandHandlers;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddInfisicalSecretsProvider();

builder.Host.UseNLogLogging();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
                      builder =>
                      {
                          builder.AllowAnyMethod()
                                 .AllowAnyHeader();

                          builder.AllowAnyOrigin();
                      });
});

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
        });

builder.Services.AddJwtBearerAuthentication(builder.Configuration);

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

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddExceptionHandler<UnhandledExceptionResponseHandler>();
builder.Services.AddProblemDetails();

var assemblies = AppDomain.CurrentDomain
                          .GetAssemblies()
                          .FilterSkippedAssemblies()
                          .ToArray();
// Web Socket Services
builder.Services.AddWebSocketCommandServices(builder.Configuration, assemblies);
builder.Services.AddWebSocketAuthTokenGenerator<WebSocketAuthTokenGenerator>();
builder.Services.AddWebSocketJwtBearerMessageHandler();

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

app.UseCors("CorsPolicy");

app.UseAllAuthenticators();

app.UseAuthorization();

app.UseWebSocketAuthRequestEndpoint();

app.UseWebSocketCommandServices();

app.MapControllers();

app.MapClientTelemetryEndpoint("api/telemetry");

app.MapErrorDocsTrackerUI(options =>
{
    options.UiPath = "/errors";
    options.ApplicationName = "Test Error";
});

app.MapGet("/api/login",
           async (string           name,
                  HttpContext      context,
                  JwtConfiguration jwtConfig) =>
           {
               var claims = new List<Claim>
               {
                   new Claim(ClaimTypes.Name, name),
                   new Claim(ClaimTypes.Role, "Admin")
               };

               var authToken = jwtConfig.CreateAuthenticationToken(claims,
                                                                   out var expiration,
                                                                   "localhost:8080");

               return Results.Ok(authToken);
           });

app.MapGet("/api/testMessage",
           async (string           name,
                  IConnectionManager connectionManager,
                  CancellationToken cancellationToken) =>
           {
               await connectionManager.DeliverMessageToAllChannels(new ResponseMessage
               {
                   Action = "helloFromServer",
                   Payload = new Dictionary<string, object?>()
                   {
                       {"Message", "Hey, this is sent from server"}
                   }
               }, cancellationToken);

               return Results.Ok();
           });

app.Run();
