using DotNetBrightener.Caching.Memory;
using DotNetBrightener.SiteSettings;
using DotNetBrightener.SiteSettings.Extensions;
using DotNetBrightener.SiteSettings.WebApp;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLocalization();

// Add services to the container.
var mvcBuilder = builder.Services.AddControllersWithViews();

mvcBuilder.RegisterSiteSettingApi();

builder.Services
       .AddSiteSettingsSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Services.RegisterSettingType<DemoSiteSetting>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapControllers();

app.Run();