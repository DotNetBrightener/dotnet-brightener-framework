using AspNet.Extensions.SelfDocumentedProblemResult.ExceptionHandlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// builder.Services.AddCommonServices(builder.Configuration);


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

app.MapErrorDocsTrackerUI(options =>
{
    options.UiPath = "/errors";
    options.ApplicationName = "Test Error";
});

app.Run();
