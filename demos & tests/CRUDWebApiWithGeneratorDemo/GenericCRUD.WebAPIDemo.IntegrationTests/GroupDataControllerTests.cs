using CRUDWebApiWithGeneratorDemo;
using CRUDWebApiWithGeneratorDemo.Database.DbContexts;
using DotNetBrightener.TestHelpers;
using FluentAssertions;
using GenericCRUD.WebAPIDemo.IntegrationTests.StartupTasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Net;
using Xunit.Abstractions;

namespace GenericCRUD.WebAPIDemo.IntegrationTests;

public class GroupDataControllerTestsFactory : MsSqlWebApiTestFactory<CRUDWebApiGeneratorRegistration, MainAppDbContext>
{
    protected override void ConfigureTestServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddHostedService<GroupDataSeeding>();
    }
}

public class GroupDataControllerTests(
    GroupDataControllerTestsFactory apiFactory,
    ITestOutputHelper               testOutputHelper)
    : IClassFixture<GroupDataControllerTestsFactory>
{
    private readonly HttpClient _client = apiFactory.CreateClient();

    [Fact]
    public async Task GroupEntity_GetList_WithoutFilter_ShouldReturnData()
    {
        var response = await _client.GetAsync("/api/GroupEntity?columns=name,createdBy,createdDate");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseString = await response.Content.ReadAsStringAsync();

        var jobject = JArray.Parse(responseString);

        jobject.Count.Should().BeGreaterThan(1);

        // validate the JSON to be return with only the requested columns
        foreach (var item in jobject)
        {
            var itemDictionary = item.ToObject<Dictionary<string, object>>();

            itemDictionary.Should().Contain(c => c.Key == "name", "The query requests for it");
            itemDictionary.Should().Contain(c => c.Key == "createdDate", "The query requests for it");
            itemDictionary.Should().Contain(c => c.Key == "createdBy", "The query requests for it");
        }
    }

    [Fact]
    public async Task GroupEntity_GetList_WithFilter_Between_ShouldReturnData()
    {
        var denverJuly5thStart = DateTimeOffset.Parse("2024-07-05T00:00:00-06:00");
        var denverJuly5thEnd   = DateTimeOffset.Parse("2024-07-05T23:59:59-06:00");

        var response = await _client.GetAsync($"/api/GroupEntity?" +
                                              $"columns=name,createdBy,createdDate" +
                                              $"&createdDate=ge({denverJuly5thStart:O})" +
                                              $"&createdDate=le({denverJuly5thEnd:O})");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseString = await response.Content.ReadAsStringAsync();

        var jobject = JArray.Parse(responseString);

        jobject.Count.Should().Be(24);

        // validate the JSON to be return with only the requested columns
        foreach (var item in jobject)
        {
            var itemDictionary = item.ToObject<Dictionary<string, object>>();

            itemDictionary.Should().Contain(c => c.Key == "name", "The query requests for it");
            itemDictionary.Should().Contain(c => c.Key == "createdDate", "The query requests for it");
            itemDictionary.Should().Contain(c => c.Key == "createdBy", "The query requests for it");
        }
    }

    [Fact]
    public async Task GroupEntity_GetList_WithFilter_DateOn_ShouldReturnData()
    {
        var response = await _client.GetAsync($"/api/GroupEntity?" +
                                              $"columns=name,createdBy,createdDate" +
                                              $"&createdDate=on(2024-07-05T00:00:00.000-06:00)");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseString = await response.Content.ReadAsStringAsync();

        var jobject = JArray.Parse(responseString);

        jobject.Count.Should().Be(24);

        // validate the JSON to be return with only the requested columns
        foreach (var item in jobject)
        {
            var itemDictionary = item.ToObject<Dictionary<string, object>>();

            itemDictionary.Should().NotBeNull();
            itemDictionary.Should().Contain(c => c.Key == "name", "The query requests for it");
            itemDictionary.Should().Contain(c => c.Key == "createdDate", "The query requests for it");
            itemDictionary.Should().Contain(c => c.Key == "createdBy", "The query requests for it");

            testOutputHelper.WriteLine(itemDictionary!["name"] + ", created on " + itemDictionary["createdDate"]);
        }
    }

    [Fact]
    public async Task GroupEntity_GetList_WithFilter_DateNotOn_ShouldReturnData()
    {
        var response = await _client.GetAsync($"/api/GroupEntity?" +
                                              $"columns=name,createdBy,createdDate" +
                                              $"&createdDate=!on(2024-07-06T00:00:00.000-06:00)");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseString = await response.Content.ReadAsStringAsync();

        var jobject = JArray.Parse(responseString);

        jobject.Count.Should().Be(48);

        // validate the JSON to be return with only the requested columns
        foreach (var item in jobject)
        {
            var itemDictionary = item.ToObject<Dictionary<string, object>>();

            itemDictionary.Should().Contain(c => c.Key == "name", "The query requests for it");
            itemDictionary.Should().Contain(c => c.Key == "createdDate", "The query requests for it");
            itemDictionary.Should().Contain(c => c.Key == "createdBy", "The query requests for it");

            testOutputHelper.WriteLine(itemDictionary["name"] + ", created on " + itemDictionary["createdDate"]);
        }
    }

    [Fact]
    public async Task GroupEntity_GetList_WithFilter_DateOn_WrongFormat_ShouldThrowException()
    {
        var response = await _client.GetAsync($"/api/GroupEntity?" +
                                              $"columns=name,createdBy,createdDate" +
                                              $"&createdDate=!on(2024-07-06T12:00:00.000-06:00)");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseString = await response.Content.ReadAsStringAsync();

        var jobject = JObject.Parse(responseString);

        var itemDictionary = jobject.ToObject<Dictionary<string, object>>();
        itemDictionary.Should().Contain(c => c.Key == "detail", "should describe error detail");
        jobject["detail"]!.ToString().Should().Be("For ON / NOT ON operators, the date value must be at 00:00:00");
    }

    [Fact]
    public async Task GroupEntity_GetList_WithFilter_DateOn_NoTimezoneInfo_ShouldThrowException()
    {
        var response = await _client.GetAsync($"/api/GroupEntity?" +
                                              $"columns=name,createdBy,createdDate" +
                                              $"&createdDate=!on(2024-07-06T00:00:00.000)");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseString = await response.Content.ReadAsStringAsync();

        var jobject = JObject.Parse(responseString);

        var itemDictionary = jobject.ToObject<Dictionary<string, object>>();
        itemDictionary.Should().Contain(c => c.Key == "detail", "should describe error detail");
        jobject["detail"].ToString().Should().Be("No timezone info provided");
    }

    [Theory]
    [InlineData("eq")]
    [InlineData("ne")]
    [InlineData("sw")]
    [InlineData("!sw")]
    [InlineData("ew")]
    [InlineData("!ew")]
    public async Task GroupEntity_GetList_NotSupportedOperator_ShouldThrowException(string operation)
    {
        var response = await _client.GetAsync($"/api/GroupEntity?" +
                                              $"columns=name,createdBy,createdDate" +
                                              $"&createdDate={operation}(whatevervalue)");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseString = await response.Content.ReadAsStringAsync();

        var jobject = JObject.Parse(responseString);

        var itemDictionary = jobject.ToObject<Dictionary<string, object>>();
        itemDictionary.Should().Contain(c => c.Key == "detail", "should describe error detail");
        jobject["detail"].ToString().Should().Contain("is not supported for filtering by property");
    }
}