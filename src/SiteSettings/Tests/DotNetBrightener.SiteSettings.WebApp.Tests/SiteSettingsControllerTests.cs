using DotNetBrightener.SiteSettings.Data.Mssql.Data;
using DotNetBrightener.SiteSettings.Models;
using DotNetBrightener.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace DotNetBrightener.SiteSettings.WebApp.Tests;

public class SiteSettingsControllerTestFactory : MsSqlWebApiTestFactory<ISiteSettingsWebApp>
{
    protected override void ConfigureTestServices(IServiceCollection serviceCollection)
    {
        ReplaceDbContextOption<MssqlStorageSiteSettingDbContext>(serviceCollection);
    }
}

public class SiteSettingsControllerTests(SiteSettingsControllerTestFactory apiFactory)
    : IClassFixture<SiteSettingsControllerTestFactory>
{
    private readonly HttpClient _client = apiFactory.CreateClient();

    [Fact]
    public async Task GetAllSettings_ShouldReturn_NoEmpty()
    {
        var response = await _client.GetAsync("/api/siteSettings/allSettings");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var settings = await response.Content.ReadFromJsonAsync<List<SettingDescriptorModel>>();

        settings.Should().NotBeEmpty();

        settings!.FirstOrDefault(x => x.SettingType == typeof(DemoSiteSetting).FullName).Should().NotBeNull();
    }

    [Fact]
    public async Task GetSettings_ShouldReturn()
    {
        var response = await _client.GetAsync($"/api/siteSettings/{typeof(DemoSiteSetting).FullName}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var settings = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        settings.Should().NotBeNull();

        settings!.All(k => k.Key != "settingContent").Should().BeTrue();
        settings!.All(k => k.Key != "settingType").Should().BeTrue();
        settings!.All(k => k.Key != "SettingContent").Should().BeTrue();
        settings!.All(k => k.Key != "SettingType").Should().BeTrue();
    }

    [Fact]
    public async Task GetSettings_UpdateSettings_ShouldReturn_ChangedValue()
    {
        DemoSiteSetting updatedValue = new()
        {
            Defaultvalue = "Updated value",
            IntSetting = 10,
            StringSetting = "Has value"
        };

        var jsonContent = JsonConvert.SerializeObject(updatedValue);
        var updateResponse = await _client.PostAsync($"/api/siteSettings/{typeof(DemoSiteSetting).FullName}",
                                                     new StringContent(jsonContent, Encoding.UTF8, "application/json"));

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // validate the updated value
        {
            var response = await _client.GetAsync($"/api/siteSettings/{typeof(DemoSiteSetting).FullName}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var settings = await response.Content.ReadFromJsonAsync<DemoSiteSetting>();

            settings.Should().NotBeNull();
            settings!.Defaultvalue.Should().Be("Updated value");
        }

        // validate the original value
        {
            var response = await _client.GetAsync($"/api/siteSettings/{typeof(DemoSiteSetting).FullName}/default");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var settings = await response.Content.ReadFromJsonAsync<DemoSiteSetting>();

            settings.Should().NotBeNull();
            settings!.Defaultvalue.Should().Be("Default Value");
        }
    }

    [Fact]
    public async Task GetSettings_WrongSettingType_ShouldReturnError()
    {
        var response = await _client.GetAsync($"/api/siteSettings/This.Is.Non.Exist.Settings");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}