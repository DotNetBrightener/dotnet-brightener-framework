using CRUDApiDemo.DemoServices;
using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.EF.Extensions;
using DotNetBrightener.Infrastructure.JwtAuthentication;
using DotNetBrightener.Infrastructure.JwtAuthentication.Middlewares;
using DotNetBrightener.WebApp.CommonShared.Extensions;
using Microsoft.EntityFrameworkCore;

const string connectionString = $"Data Source=TestDb.db;";

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseNLogLogging();

// Add services to the container.
builder.Services.AddCommonServices(builder.Configuration);

builder.Services.AddEntityFrameworkDataServices<DemoDbContext>(new DatabaseConfiguration
                                                               {
                                                                   ConnectionString = connectionString,
                                                                   DatabaseProvider = DatabaseProvider.Sqlite
                                                               },
                                                               optionBuilder =>
                                                               {
                                                                   optionBuilder.UseSqlite(connectionString);
                                                               });

builder.Services.AddCommonMvcApp();

builder.Services.AddJwtBearerAuthentication(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// auto-detect classes that implement IDependency / ISingletonDependency and register them
var applicationAssemblies = AppDomain.CurrentDomain.GetAssemblies();
builder.Services.AutoRegisterDependencyServices(applicationAssemblies);

// enable lazy resolving dependencies
builder.Services.EnableLazyResolver();

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<AllSchemesAuthenticationMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
