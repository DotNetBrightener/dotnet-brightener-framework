using DotNetBrightener.SyncServer;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSecuredApi();

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/client",
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "client"))
});

app.UseSecureApiHandle();
app.MapSecuredPut<SyncUserService>("synchronize/syncUser");

app.Run();