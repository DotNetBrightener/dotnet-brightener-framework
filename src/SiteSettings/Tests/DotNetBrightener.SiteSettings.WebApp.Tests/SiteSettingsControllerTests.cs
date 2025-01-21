using DotNetBrightener.SiteSettings.Data.Mssql.Data;
using DotNetBrightener.SiteSettings.Models;
using DotNetBrightener.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Shouldly;

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

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var settings = await response.Content.ReadFromJsonAsync<List<SettingDescriptorModel>>();

        settings.ShouldNotBeEmpty();

        settings!.FirstOrDefault(x => x.SettingType == typeof(DemoSiteSetting).FullName).ShouldNotBeNull();
    }

    [Fact]
    public async Task GetSettings_ShouldReturn_Without_Unneeded_Fields()
    {
        var response = await _client.GetAsync($"/api/siteSettings/{typeof(DemoSiteSetting).FullName}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var settings = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        settings.ShouldNotBeNull();

        new List<string>
            {
                "settingContent",
                "settingType",
                "SettingContent",
                "SettingType"
            }
           .ForEach(s =>
            {
                settings.Keys.ShouldNotContain(s);
            });
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

        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // validate the updated value
        {
            var response = await _client.GetAsync($"/api/siteSettings/{typeof(DemoSiteSetting).FullName}");

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var settings = await response.Content.ReadFromJsonAsync<DemoSiteSetting>();

            settings.ShouldNotBeNull();
            settings!.Defaultvalue.ShouldBe("Updated value");
        }

        // validate the original value
        {
            var response = await _client.GetAsync($"/api/siteSettings/{typeof(DemoSiteSetting).FullName}/default");

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var settings = await response.Content.ReadFromJsonAsync<DemoSiteSetting>();

            settings.ShouldNotBeNull();
            settings!.Defaultvalue.ShouldBe("Default Value");
        }
    }

    [Fact]
    public async Task GetSettings_WrongSettingType_ShouldReturnError()
    {
        var response = await _client.GetAsync($"/api/siteSettings/This.Is.Non.Exist.Settings");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}