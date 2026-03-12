using ActivityLog.DataStorage;
using ActivityLogTest.ApiServer;
using DotNetBrightener.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITestActivity, TestActivity>();

builder.Services.TryAddScoped<ScopedCurrentUserResolver>();
builder.Services.TryAddScoped<ICurrentLoggedInUserResolver, DefaultCurrentUserResolver>();

var assemblies = AppDomain.CurrentDomain.GetAssemblies();

builder.Services
       .AddActivityLogging(builder.Configuration,
                           assembliesToScan: assemblies)
       .WithStorage()
       .UsePostgreSql(builder.Configuration.GetConnectionString("DefaultConnection"));


var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapGet("/test-audit-log/{testId}",
           async (int           testId,
                  HttpContext   httpContext,
                  ITestActivity testActivity) =>
           {
               await testActivity.DoSomething(testId);

               return Results.Ok();
           });

app.MapGet("/test-audit-log",
           async (int           testId,
                  HttpContext   httpContext,
                  ITestActivity testActivity) =>
           {
               await testActivity.DoSomething2(new TestClass
               {
                   ProductName = "test prd"
               });

               return Results.Ok();
           });


app.Run();