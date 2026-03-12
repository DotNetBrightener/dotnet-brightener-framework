using AspNet.Extensions.SelfDocumentedProblemResult.ExceptionHandlers;
using DotNetBrightener.DataAccess;
using DotNetBrightener.Infrastructure.JwtAuthentication;
using DotNetBrightener.WebSocketExt.Messages;
using DotNetBrightener.WebSocketExt.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json.Serialization;
using DotNetBrightener.SiteSettings;
using WebAppCommonShared.Demo.DbContexts;
using WebAppCommonShared.Demo.WebSocketCommandHandlers;


var builder = WebApplication.CreateBuilder(args);
ExtendedServiceFactory.ApplyServiceProviderFactory(builder.Host);

builder.Configuration.AddInfisicalSecretsProvider();

builder.Host.UseNLogLogging();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new
        Exception("No connection string configured. Please add 'DefaultConnection' connection string with valid value to the Environment or appsettings.json file.");
}

builder.Services
       .ConfigureLogging(builder.Configuration);

builder.EnableOpenTelemetry("https://otlpendpoint.dotnetbrightener.com", "SfVxZSrjOELVVUIzxy63GhQbobFGg2ZZsA480IBQ7pPBzDs4k03RGgfkxgVhrTrK");

builder.Services
       .AddTemplateEngine()
       .AddTemplateEngineStorage()
       .AddTemplateEngineSqlServerStorage(connectionString);

var mvcBuilder = builder.Services.AddControllers();
builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

mvcBuilder.RegisterSiteSettingApi();
builder.Services.AddSiteSettingsSqlServerStorage(connectionString);

builder.Services.AddJwtBearerAuthentication(builder.Configuration);

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
builder.Services.AddExceptionHandler<UnhandledExceptionResponseHandler>();
builder.Services.AddProblemDetails();

builder.Services.EnableBackgroundTaskServices(builder.Configuration);


//-----------------------------------------------------------------------------------------------
//  App Client Manager: To allow only registered clients to access the APIs
//-----------------------------------------------------------------------------------------------
var appClientManagerBuilder = builder.Services.AddAppClientManager(builder.Configuration);

appClientManagerBuilder.WithStorage()
                       .UseSqlServer(dbConfiguration.ConnectionString!);

builder.Services.AddAppClientAudienceValidator();

builder.Services.AddEFCentralizedDataServices<MainAppDbContext>(dbConfiguration,
                                                                builder.Configuration,
                                                                configureDatabase);

var assemblies = AppDomain.CurrentDomain
                          .GetAppOnlyAssemblies();


//-----------------------------------------------------------------------------------------------
//  Enable Web Socket Support
//-----------------------------------------------------------------------------------------------
builder.Services.AddWebSocketCommandServices(builder.Configuration, assemblies);
builder.Services.AddWebSocketAuthTokenGenerator<WebSocketAuthTokenGenerator>();
builder.Services.AddWebSocketJwtBearerMessageHandler();

builder.Services.EnableLazyResolver();

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

//-----------------------------------------------------------------------------------------------
//  App Client Manager: Enable CORS for registered clients only
//-----------------------------------------------------------------------------------------------
app.UseAppClientCorsPolicy();

app.UseAllAuthenticators();

app.UseAuthorization();

app.UseWebSocketAuthRequestEndpoint();

app.UseWebSocketCommandServices();

app.MapControllers();

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
                                                                   out var expiration);

               return Results.Ok(new
               {
                   access_token = authToken,
                   expires_at   = expiration
               });
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
