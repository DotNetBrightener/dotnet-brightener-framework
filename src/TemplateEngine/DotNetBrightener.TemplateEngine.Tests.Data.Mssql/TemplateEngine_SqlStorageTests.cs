using DotNetBrightener.TemplateEngine.Data.Mssql.Data;
using DotNetBrightener.TemplateEngine.Data.Services;
using DotNetBrightener.TemplateEngine.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using Testcontainers.MsSql;

namespace DotNetBrightener.TemplateEngine.Tests.Data.Mssql;

[TestFixture]
public class TemplateEngine_SqlStorageTests : MsSqlServerBaseTest
{
    private IHost _testHost;

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
            services.Replace(ServiceDescriptor.Scoped(_ => mockTemplateHelper.Object));
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
            services.AddTemplateEngineSqlServerStorage(ConnectionString);
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