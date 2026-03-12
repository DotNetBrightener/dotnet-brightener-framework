using DotNetBrightener.TemplateEngine.Data.Services;
using DotNetBrightener.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.TemplateEngine.Tests.Data.Mssql;

public class TemplateEngine_SqlStorageTests(ITestOutputHelper testOutputHelper) : MsSqlServerBaseXUnitTest(testOutputHelper)
{
    private IHost _testHost;

    [Fact]
    public async Task TemplateProvider_ShouldCreateANewTemplateRecord()
    {
        _testHost = HostTestingHelper.CreateTestHost(testOutputHelper, services =>
        {
            services.AddTemplateEngineStorage();
            services.AddTemplateEngineSqlServerStorage(ConnectionString);
            services.AddTemplateProvider<TestModelRegistration>();
        });

        await _testHost.StartAsync();

        await Task.Delay(TimeSpan.FromSeconds(10));

        using (var scope = _testHost.Services.CreateScope())
        {
            var templateService = scope.ServiceProvider.GetRequiredService<ITemplateService>();

            var template = await templateService.LoadTemplateAsync<TemplateTestModel>();

            template.ShouldNotBeNull().ShouldNotBeNull();
            template.TemplateTitle.ShouldBe("Hello {{Name}}"); 
            template.TemplateContent.ShouldBe("Hey {{Name}},<br />This is {{Description}}.");
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

            template.ShouldNotBeNull();
            template.TemplateTitle.ShouldBe("Hello John");
            template.TemplateContent.ShouldBe("Hey John,<br />This is A test model.");
        }

        await _testHost.StopAsync();
    }
}