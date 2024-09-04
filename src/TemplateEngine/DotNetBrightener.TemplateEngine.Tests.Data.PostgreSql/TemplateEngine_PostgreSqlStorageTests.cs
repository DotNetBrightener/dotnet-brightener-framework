using DotNetBrightener.TemplateEngine.Data.PostgreSql.Data;
using DotNetBrightener.TemplateEngine.Data.Services;
using DotNetBrightener.TemplateEngine.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace DotNetBrightener.TemplateEngine.Tests.Data.PostgreSql;

public class TemplateEngine_PostgreSqlStorageTests
{
    private IHost _testHost;
    private string _connectionString;

    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
                                                               .WithImage("postgres:15")
                                                               .WithDatabase($"DataMigration_UnitTest{DateTime.Now:yyyyMMddHHmm}")
                                                               .WithUsername("test")
                                                               .WithPassword("password")
                                                               .Build();


    [SetUp]
    public async Task Setup()
    {
        await _postgreSqlContainer.StartAsync();
        _connectionString = _postgreSqlContainer.GetConnectionString();
    }

    [TearDown]
    public async Task TearDown()
    {
        TearDownHost();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    private void TearDownHost()
    {
        var builder = new HostBuilder()
           .ConfigureServices((_, serviceCollection) =>
            {
                serviceCollection.AddDbContext<TemplateEngineDbContext>(options =>
                {
                    options.UseNpgsql(_postgreSqlContainer.GetConnectionString());
                });
            });

        var host = builder.Build();

        using var serviceScope    = host.Services.CreateScope();
        var       serviceProvider = serviceScope.ServiceProvider;

        using var dbContext = serviceProvider.GetRequiredService<TemplateEngineDbContext>();
        dbContext.Database.EnsureDeleted();
        _testHost?.Dispose();
    }

    [Test]
    public async Task TemplateHelperProvider_ShouldBeCalledAtStartup()
    {
        var mockTemplateHelper = new Mock<ITemplateHelperRegistration>();

        _testHost = HostTestingHelper.CreateTestHost(services =>
        {
            services.Replace(ServiceDescriptor.Scoped<ITemplateHelperRegistration>((s) => mockTemplateHelper.Object));
        });

        await _testHost.StartAsync();

        mockTemplateHelper.Verify(x => x.RegisterHelpers(),
                                  Times.Once);

        await _testHost.StopAsync();
    }

    [Test]
    public async Task TemplateProvider_ShouldCreateANewTemplateRecord()
    {
        _testHost = HostTestingHelper.CreateTestHost(services =>
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

            Assert.That(template, Is.Not.Null);
            Assert.That(template.TemplateTitle, Is.EqualTo("Hello {{Name}}"));
            Assert.That(template.TemplateContent, Is.EqualTo("Hey {{Name}},<br />This is {{Description}}."));
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

            Assert.That(template, Is.Not.Null);
            Assert.That(template.TemplateTitle, Is.EqualTo("Hello John"));
            Assert.That(template.TemplateContent, Is.EqualTo("Hey John,<br />This is A test model."));
        }

        await _testHost.StopAsync();
    }
}