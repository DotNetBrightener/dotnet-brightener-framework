using DotNetBrightener.TemplateEngine.Data.Services;
using DotNetBrightener.TemplateEngine.Models;
using DotNetBrightener.TemplateEngine.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;

namespace DotNetBrightener.TemplateEngine.Tests.Data;

internal class TemplateTestModel : ITemplateModel
{
    public string Name { get; set; }

    public string Description { get; set; }
}

internal class TestModelRegistration : ITemplateProvider
{
    public async Task RegisterTemplates(ITemplateStore templateStore)
    {
        await templateStore.RegisterTemplate<TemplateTestModel>("Hello {{Name}}",
                                                                "Hey {{Name}},<br />This is {{Description}}.");
    }
}

[TestFixture]
public class TemplateEngine_PhysicalStorageTests
{
    private readonly List<string> _pathsToCleanUp = new List<string>();

    private IHost _testHost;


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
    public async Task TemplateProvider_ShouldBeCalledAtStartup()
    {
        var mockTemplateProvider = new Mock<ITemplateProvider>();

        _testHost = HostTestingHelper.CreateTestHost(services =>
        {
            services.AddTemplateEngineStorage();
            services.AddScoped<ITemplateProvider>((s) => mockTemplateProvider.Object);
        });

        await _testHost.StartAsync();

        mockTemplateProvider.Verify(x => x.RegisterTemplates(It.IsAny<ITemplateStore>()),
                                    Times.Once);

        await _testHost.StopAsync();
    }

    [Test]
    public async Task TemplateProvider_ShouldCreateANewTemplateFile()
    {
        _testHost = HostTestingHelper.CreateTestHost(services =>
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


        Assert.That(File.Exists(templatesPath), Is.True);

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