using DotNetBrightener.InfisicalVaultClient;
using WebAppCommonShared.Demo;


static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseNLogLogging()
        .ConfigureAppConfiguration((hostingContext, configurationBuilder) =>
        {
            configurationBuilder.AddInfisicalSecretsProvider();
        })
        .ConfigureWebHostDefaults(webBuilder =>
         {
             webBuilder.UseStartup<Startup>();
         });


var app = CreateHostBuilder(args).Build();

app.Run();

//var builder = WebApplication.CreateBuilder(new WebApplicationOptions
//{
    
//});

//// Add services to the container.

//builder.Host
//       .ConfigureAppConfiguration((hostingContext, config) =>
//        {
//            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection",
//                                               "Server=100.121.179.124;Database=LoggingDbTest;User Id=sa;Password=sCpTXbW8jbSbbUpILfZVulTiwqcPyJWt;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;");
//        });


//builder.Host.UseNLogLogging();

//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//builder.Services
//       .ConfigureLogging(builder.Configuration)
//       .AddLogStorage(optionsBuilder =>
//        {
//            optionsBuilder.UseSqlServer(connectionString);
//        });

//builder.Services.AddControllers();

//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var dbConfiguration = new DatabaseConfiguration
//{
//    ConnectionString = connectionString,
//    UseLazyLoading   = true,
//    DatabaseProvider = DatabaseProvider.MsSql
//};

//Action<DbContextOptionsBuilder> configureDatabase = optionsBuilder =>
//{
//    optionsBuilder.UseSqlServer(dbConfiguration.ConnectionString);
//};


//builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
//builder.Services.AddExceptionHandler<UnhandledExceptionResponseHandler>();

//builder.Services.AddProblemDetails();

//var app = builder.Build();

//app.UseForwardedHeaders();

//if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
//{
//    app.UseHttpsRedirection();
//}

//app.UseExceptionHandler();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.MapClientTelemetryEndpoint("api/telemetry");

//app.MapErrorDocsTrackerUI(options =>
//{
//    options.UiPath = "/errors";
//    options.ApplicationName = "Test Error";
//});

//app.Run();
