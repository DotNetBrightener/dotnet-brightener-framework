using CRUDWebApiWithGeneratorDemo;
using CRUDWebApiWithGeneratorDemo.Database.DbContexts;
using DotNetBrightener.TestHelpers;
using GenericCRUD.WebAPIDemo.IntegrationTests.StartupTasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Shouldly;
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

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseString = await response.Content.ReadAsStringAsync();

        var jobject = JArray.Parse(responseString);

        jobject.Count.ShouldBeGreaterThan(1);

        // validate the JSON to be return with only the requested columns
        foreach (var item in jobject)
        {
            var itemDictionary = item.ToObject<Dictionary<string, object>>();

            itemDictionary.ShouldContain(c => c.Key == "name", "The query requests for it");
            itemDictionary.ShouldContain(c => c.Key == "createdDate", "The query requests for it");
            itemDictionary.ShouldContain(c => c.Key == "createdBy", "The query requests for it");
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

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseString = await response.Content.ReadAsStringAsync();

        var jobject = JArray.Parse(responseString);

        jobject.Count.ShouldBe(24);

        // validate the JSON to be return with only the requested columns
        foreach (var item in jobject)
        {
            var itemDictionary = item.ToObject<Dictionary<string, object>>();

            itemDictionary.ShouldContain(c => c.Key == "name", "The query requests for it");
            itemDictionary.ShouldContain(c => c.Key == "createdDate", "The query requests for it");
            itemDictionary.ShouldContain(c => c.Key == "createdBy", "The query requests for it");
        }
    }

    [Fact]
    public async Task GroupEntity_GetList_WithFilter_DateOn_ShouldReturnData()
    {
        var response = await _client.GetAsync($"/api/GroupEntity?" +
                                              $"columns=name,createdBy,createdDate" +
                                              $"&createdDate=on(2024-07-05T00:00:00.000-06:00)");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseString = await response.Content.ReadAsStringAsync();

        var jobject = JArray.Parse(responseString);

        jobject.Count.ShouldBe(24);

        // validate the JSON to be return with only the requested columns
        foreach (var item in jobject)
        {
            var itemDictionary = item.ToObject<Dictionary<string, object>>();

            itemDictionary.ShouldNotBeNull();
            itemDictionary.ShouldContain(c => c.Key == "name", "The query requests for it");
            itemDictionary.ShouldContain(c => c.Key == "createdDate", "The query requests for it");
            itemDictionary.ShouldContain(c => c.Key == "createdBy", "The query requests for it");

            testOutputHelper.WriteLine(itemDictionary!["name"] + ", created on " + itemDictionary["createdDate"]);
        }
    }

    [Fact]
    public async Task GroupEntity_GetList_WithFilter_DateNotOn_ShouldReturnData()
    {
        var response = await _client.GetAsync($"/api/GroupEntity?" +
                                              $"columns=name,createdBy,createdDate" +
                                              $"&createdDate=!on(2024-07-06T00:00:00.000-06:00)");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseString = await response.Content.ReadAsStringAsync();

        var jobject = JArray.Parse(responseString);

        jobject.Count.ShouldBe(48);

        // validate the JSON to be return with only the requested columns
        foreach (var item in jobject)
        {
            var itemDictionary = item.ToObject<Dictionary<string, object>>();

            itemDictionary.ShouldContain(c => c.Key == "name", "The query requests for it");
            itemDictionary.ShouldContain(c => c.Key == "createdDate", "The query requests for it");
            itemDictionary.ShouldContain(c => c.Key == "createdBy", "The query requests for it");

            testOutputHelper.WriteLine(itemDictionary["name"] + ", created on " + itemDictionary["createdDate"]);
        }
    }

    [Fact]
    public async Task GroupEntity_GetList_WithFilter_DateOn_WrongFormat_ShouldThrowException()
    {
        var response = await _client.GetAsync($"/api/GroupEntity?" +
                                              $"columns=name,createdBy,createdDate" +
                                              $"&createdDate=!on(2024-07-06T12:00:00.000-06:00)");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var responseString = await response.Content.ReadAsStringAsync();

        var jobject = JObject.Parse(responseString);

        var itemDictionary = jobject.ToObject<Dictionary<string, object>>();
        itemDictionary.ShouldContain(c => c.Key == "detail", "should describe error detail");
        jobject["detail"]!.ToString().ShouldBe("For ON / NOT ON operators, the date value must be at 00:00:00");
    }

    [Fact]
    public async Task GroupEntity_GetList_WithFilter_DateOn_NoTimezoneInfo_ShouldThrowException()
    {
        var response = await _client.GetAsync($"/api/GroupEntity?" +
                                              $"columns=name,createdBy,createdDate" +
                                              $"&createdDate=!on(2024-07-06T00:00:00.000)");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var responseString = await response.Content.ReadAsStringAsync();

        var jobject = JObject.Parse(responseString);

        var itemDictionary = jobject.ToObject<Dictionary<string, object>>();
        itemDictionary.ShouldContain(c => c.Key == "detail", "should describe error detail");
        jobject["detail"].ToString().ShouldBe("No timezone info provided");
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

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var responseString = await response.Content.ReadAsStringAsync();

        var jobject = JObject.Parse(responseString);

        var itemDictionary = jobject.ToObject<Dictionary<string, object>>();
        itemDictionary.ShouldContain(c => c.Key == "detail", "should describe error detail");
        jobject["detail"].ToString().ShouldContain("is not supported for filtering by property");
    }

    [Fact]
    public async Task GroupEntity_GetList_WithFilter_DateIn_ShouldReturnData()
    {
        var startDate = DateTimeOffset.Parse("2024-07-05T00:00:00-06:00");
        var endDate   = DateTimeOffset.Parse("2024-07-06T23:59:59-06:00");

        var response = await _client.GetAsync($"/api/GroupEntity?" +
                                              $"columns=name,createdBy,createdDate" +
                                              $"&createdDate=in({startDate:O},{endDate:O})");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseString = await response.Content.ReadAsStringAsync();

        var jobject = JArray.Parse(responseString);

        jobject.Count.ShouldBe(48);

        // validate the JSON to be return with only the requested columns
        foreach (var item in jobject)
        {
            var itemDictionary = item.ToObject<Dictionary<string, object>>();

            itemDictionary.ShouldContain(c => c.Key == "name", "The query requests for it");
            itemDictionary.ShouldContain(c => c.Key == "createdDate", "The query requests for it");
            itemDictionary.ShouldContain(c => c.Key == "createdBy", "The query requests for it");

            var createdDate = DateTimeOffset.Parse(itemDictionary["createdDate"].ToString()!);
            createdDate.ShouldBeGreaterThanOrEqualTo(startDate);
            createdDate.ShouldBeLessThanOrEqualTo(endDate);

            testOutputHelper.WriteLine(itemDictionary["name"] + ", created on " + itemDictionary["createdDate"]);
        }
    }

    [Fact]
    public async Task GroupEntity_GetList_WithFilter_DateNotIn_ShouldReturnData()
    {
        var startDate = DateTimeOffset.Parse("2024-07-05T00:00:00-06:00");
        var endDate   = DateTimeOffset.Parse("2024-07-06T23:59:59-06:00");

        var response = await _client.GetAsync($"/api/GroupEntity?" +
                                              $"columns=name,createdBy,createdDate" +
                                              $"&createdDate=!in({startDate:O},{endDate:O})");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseString = await response.Content.ReadAsStringAsync();

        var jobject = JArray.Parse(responseString);

        jobject.Count.ShouldBe(24);

        // validate the JSON to be return with only the requested columns
        foreach (var item in jobject)
        {
            var itemDictionary = item.ToObject<Dictionary<string, object>>();

            itemDictionary.ShouldContain(c => c.Key == "name", "The query requests for it");
            itemDictionary.ShouldContain(c => c.Key == "createdDate", "The query requests for it");
            itemDictionary.ShouldContain(c => c.Key == "createdBy", "The query requests for it");

            var createdDate = DateTimeOffset.Parse(itemDictionary["createdDate"].ToString()!);
            (createdDate < startDate || createdDate > endDate).ShouldBeTrue();

            testOutputHelper.WriteLine(itemDictionary["name"] + ", created on " + itemDictionary["createdDate"]);
        }
    }

    [Fact]
    public async Task GroupEntity_GetList_WithFilter_DateIn_MissingEndDate_ShouldThrowException()
    {
        var startDate = DateTimeOffset.Parse("2024-07-05T00:00:00-06:00");

        var response = await _client.GetAsync($"/api/GroupEntity?" +
                                              $"columns=name,createdBy,createdDate" +
                                              $"&createdDate=in({startDate:O})");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var responseString = await response.Content.ReadAsStringAsync();

        var jobject = JObject.Parse(responseString);

        var itemDictionary = jobject.ToObject<Dictionary<string, object>>();
        itemDictionary.ShouldContain(c => c.Key == "detail", "should describe error detail");
        jobject["detail"]!.ToString().ShouldBe("IN/NOT IN operators need start and end date parameters.");
    }
}