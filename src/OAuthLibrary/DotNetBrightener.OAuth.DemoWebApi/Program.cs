using DotNetBrightener.OAuth.DependenciesInjection;
using DotNetBrightener.OAuth.Integration.Apple.Extensions;
using DotNetBrightener.OAuth.Integration.Google.Extensions;

var builder = WebApplication.CreateBuilder(args);

var serviceCollection = builder.Services;
serviceCollection.AddHttpContextAccessor();
// Add services to the container.
serviceCollection.AddControllersWithViews();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
serviceCollection.AddEndpointsApiExplorer();
serviceCollection.AddSwaggerGen();

// Add OAuth Core Services
serviceCollection.AddOAuthServices();

// Ad OAuth Providers
serviceCollection.AddGoogleAuthentication(builder.Configuration)
                 .AddAppleAuthentication(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
