using DotNetBrightener.TemplateEngine.Data.PostgreSql.Data;
using DotNetBrightener.TemplateEngine.Data.Services;
using DotNetBrightener.TemplateEngine.Services;
using DotNetBrightener.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;

namespace DotNetBrightener.TemplateEngine.Tests.Data.PostgreSql;

[IgnoreIfRunOnAzureVm]
[TestFixture]
public class TemplateEngine_PostgreSqlStorageTests
{
    private IHost  _testHost;
    private string _connectionString;

    [SetUp]
    public void Setup()
    {
        _connectionString =
            $"Server=100.117.90.128;Port=5432;Database=TemplateEngine_UnitTest{DateTime.Now:yyyyMMddHHmm};User Id=postgres;Password=Sup3r5tr0ngP@ssw0rd!!;";
    }

    [TearDown]
    public async Task TearDown()
    {
        await using (var dbContext = _testHost.Services.GetService<TemplateEngineDbContext>())
        {
            if (dbContext is not null)
            {
                await dbContext.Database.EnsureDeletedAsync();
            }
        }

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
                Name        = "John",
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