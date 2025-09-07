using DotNetBrightener.Identity;
using DotNetBrightener.Identity.Models.Defaults;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Identity services
try
{
    builder.Services.AddIdentity<IdentityUser, IdentityRole, IdentityAccount>();
    builder.Logging.AddConsole();
    Console.WriteLine("✅ Identity services registered successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Could not register Identity services: {ex.Message}");
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Add a simple health check endpoint
app.MapGet("/", () => new
{
    Application = "DotNetBrightener.Identity Demo WebAPI",
    Status = "Running",
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName
});

// Add Identity module information endpoint
app.MapGet("/identity/info", () =>
{
    try
    {
        var assembly = typeof(IdentityUser).Assembly;
        var types = assembly.GetTypes();
        var modelTypes = types.Where(t => t.Namespace?.Contains("Models") == true).ToList();
        var serviceTypes = types.Where(t => t.Namespace?.Contains("Services") == true).ToList();
        var dbContextTypes = types.Where(t => t.Name.Contains("DbContext")).ToList();

        return Results.Ok(new
        {
            Assembly = assembly.FullName,
            Version = assembly.GetName().Version?.ToString(),
            TotalTypes = types.Length,
            ModelTypes = modelTypes.Count,
            ServiceTypes = serviceTypes.Count,
            DbContextTypes = dbContextTypes.Count,
            KeyModelTypes = modelTypes.Take(10).Select(t => t.Name).ToList(),
            KeyServiceTypes = serviceTypes.Take(10).Select(t => t.Name).ToList()
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error loading Identity module info: {ex.Message}");
    }
});

app.Run();
