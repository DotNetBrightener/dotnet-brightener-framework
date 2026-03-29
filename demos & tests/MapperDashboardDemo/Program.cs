using DotNetBrightener.Mapper.Dashboard;
using DotNetBrightener.Mapper.Mapping;
using MapperDashboardDemo.Endpoints;
using MapperDashboardDemo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDotNetBrightenerMapperDashboard(options =>
{
    options.Title = "DotNetBrightener Mapper Dashboard Demo";
    options.RoutePrefix = "/dnb-mapper";
    options.AccentColor = "#6366f1";
    options.DefaultDarkMode = false;
    options.EnableJsonApi = true;
});

builder.Services.AddSingleton<SeedDataService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapDotNetBrightenerMapperDashboard();

app.MapUserEndpoints();
app.MapProductEndpoints();
app.MapCompanyEndpoints();
app.MapOrderEndpoints();
app.MapBlogPostEndpoints();

app.Run();
