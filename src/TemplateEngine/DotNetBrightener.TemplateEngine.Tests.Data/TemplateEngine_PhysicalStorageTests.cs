using DotNetBrightener.TemplateEngine.Data.Services;
using DotNetBrightener.TemplateEngine.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.TemplateEngine.Tests.Data;

public class TemplateEngine_PhysicalStorageTests(ITestOutputHelper testOutputHelper)
{
    private readonly List<string> _pathsToCleanUp = new List<string>();

    private IHost _testHost;
    
    [Fact]
    public async Task TemplateHelperProvider_ShouldBeCalledAtStartup()
    {
        var mockTemplateHelper = new Mock<ITemplateHelperRegistration>();

        _testHost = HostTestingHelper.CreateTestHost(testOutputHelper, services =>
        {
            services.Replace(ServiceDescriptor.Scoped<ITemplateHelperRegistration>((s) => mockTemplateHelper.Object));
        });

        await _testHost.StartAsync();

        mockTemplateHelper.Verify(x => x.RegisterHelpers(),
                                  Times.Once);

        await _testHost.StopAsync();
    }

    [Fact]
    public async Task TemplateProvider_ShouldBeCalledAtStartup()
    {
        var mockTemplateProvider = new Mock<ITemplateProvider>();

        _testHost = HostTestingHelper.CreateTestHost(testOutputHelper, services =>
        {
            services.AddTemplateEngineStorage();
            services.AddScoped<ITemplateProvider>((s) => mockTemplateProvider.Object);
        });

        await _testHost.StartAsync();

        mockTemplateProvider.Verify(x => x.RegisterTemplates(It.IsAny<ITemplateStore>()),
                                    Times.Once);

        await _testHost.StopAsync();
    }

    [Fact]
    public async Task TemplateProvider_ShouldCreateANewTemplateFile()
    {
        _testHost = HostTestingHelper.CreateTestHost(testOutputHelper, services =>
        {
            services.AddTemplateEngineStorage();
            services.AddTemplateProvider<TestModelRegistration>();
        });

        var environment        = _testHost.Services.GetRequiredService<IHostEnvironment>();
        var templateFolderPath = Path.Combine(environment.ContentRootPath, "Templates");
        _pathsToCleanUp.Add(templateFolderPath);

        foreach (var path in _pathsToCleanUp)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        _pathsToCleanUp.Clear();

        await _testHost.StartAsync();

        var expectingFileName  = typeof(TemplateTestModel).FullName + ".tpl";
        var templatesPath      = Path.Combine(templateFolderPath, expectingFileName);


        File.Exists(templatesPath).Should().BeTrue();

        using (var scope = _testHost.Services.CreateScope())
        {
            var templateService = scope.ServiceProvider.GetRequiredService<ITemplateService>();

            var template = templateService.LoadTemplate<TemplateTestModel>();


            template.Should().NotBeNull();
            template.TemplateTitle.Should().Be("Hello {{Name}}");
            template.TemplateContent.Should().Be("Hey {{Name}},<br />This is {{Description}}.");
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

            template.Should().NotBeNull();
            template.TemplateTitle.Should().Be("Hello John");
            template.TemplateContent.Should().Be("Hey John,<br />This is A test model.");
        }
        
        await _testHost.StopAsync();
    }
}