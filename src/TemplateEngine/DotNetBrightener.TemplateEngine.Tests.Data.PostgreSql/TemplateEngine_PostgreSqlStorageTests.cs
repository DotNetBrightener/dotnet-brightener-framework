using DotNetBrightener.TemplateEngine.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Testcontainers.PostgreSql;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.TemplateEngine.Tests.Data.PostgreSql;

internal class EmptyDbContext(DbContextOptions<EmptyDbContext> options) : DbContext(options);

public class TemplateEngine_PostgreSqlStorageTests(ITestOutputHelper testOutputHelper) : IAsyncLifetime
{
    private IHost _testHost;
    private string _connectionString;

    private PostgreSqlContainer _postgreSqlContainer;

    public async Task InitializeAsync()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
                              .WithImage("postgres:15")
                              .WithDatabase($"DataMigration_UnitTest{DateTime.Now:yyyyMMddHHmm}")
                              .WithUsername("test")
                              .WithPassword("password")
                              .Build();

        await _postgreSqlContainer.StartAsync();
        _connectionString = _postgreSqlContainer.GetConnectionString();


        var builder = new HostBuilder()
           .ConfigureServices((_, serviceCollection) =>
            {
                serviceCollection.AddDbContext<EmptyDbContext>(options =>
                {
                    options.UseNpgsql(_postgreSqlContainer.GetConnectionString());
                });
            });

        var host = builder.Build();

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;

            await using (var dbContext = serviceProvider.GetRequiredService<EmptyDbContext>())
            {
                await dbContext.Database.EnsureCreatedAsync();
            }
        }
    }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }


    [Fact]
    public async Task TemplateProvider_ShouldCreateANewTemplateRecord()
    {
        _testHost = HostTestingHelper.CreateTestHost(testOutputHelper, services =>
        {
            services.AddTemplateEngineStorage();
            services.AddTemplateEnginePostgreSqlStorage(_connectionString);
            services.AddTemplateProvider<TestModelRegistration>();
        });

        await _testHost.StartAsync();

        using (var scope = _testHost.Services.CreateScope())
        {
            var templateService = scope.ServiceProvider.GetRequiredService<ITemplateService>();

            var template = templateService.LoadTemplate<TemplateTestModel>();

            template.ShouldNotBeNull();
            template.TemplateTitle.ShouldBe("Hello {{Name}}");
            template.TemplateContent.ShouldBe("Hey {{Name}},<br />This is {{Description}}.");
        }

        using (var scope = _testHost.Services.CreateScope())
        {
            var templateService = scope.ServiceProvider.GetRequiredService<ITemplateService>();

            var model = new TemplateTestModel
            {
                Name = "John",
                Description = "A test model"
            };

            var template = await templateService.LoadAndParseTemplateAsync(model);

            template.ShouldNotBeNull();
            template.TemplateTitle.ShouldBe("Hello John");
            template.TemplateContent.ShouldBe("Hey John,<br />This is A test model.");
        }

        await _testHost.StopAsync();
    }
}