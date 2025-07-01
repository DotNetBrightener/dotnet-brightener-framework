using DotNetBrightener.TestHelpers.PostgreSql;
using Microsoft.Extensions.DependencyInjection;
using MultiTenancy.Demo.WebApi.DbContexts;
using MultiTenancy.Demo.WebApi.IntegrationTests.StartupTasks;
using Newtonsoft.Json.Linq;
using Shouldly;
using System.Net;
using DotNetBrightener.MultiTenancy;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace MultiTenancy.Demo.WebApi.IntegrationTests;

public class TenantManagementControllerTestsFactory
    : PostgreSqlWebApiTestFactory<IMultiTenantApi, MultiTenancyDbContext>
{
    protected override string DatabaseName => "MultiTenancy_Demo_WebApi_IntegrationTest";

    protected override int? ExposedDatabasePort => 5433;

    protected override void ConfigureTestServices(IServiceCollection serviceCollection)
    {
        var _ = ConnectionString;

        serviceCollection.AddHostedService<UserDataSeeding>();
    }
}

public class TenantManagementControllerTests(
    TenantManagementControllerTestsFactory apiFactory,
    ITestOutputHelper                      testOutputHelper)
    : IClassFixture<TenantManagementControllerTestsFactory>
{
    [Fact]
    public async Task GetUsersByTenant_ShouldReturnCorrectDataPerTenant()
    {
        var client = apiFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri($"http://{UserDataSeeding.Clinic1.DomainsList[0]}")
        });

        var        request = new HttpRequestMessage(HttpMethod.Get, "/api/allusers/users");
        request.Headers.Add(MultiTenantHeaders.CurrentTenantId, UserDataSeeding.Clinic1.Id.ToString());

        var response = await client.SendAsync(request);

        var responseString = await response.Content.ReadAsStringAsync();

        testOutputHelper.WriteLine(responseString);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var jobject = JArray.Parse(responseString);

        jobject.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetTenants_ShouldReturnCorrectData()
    {
        var client = apiFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri($"http://{UserDataSeeding.Clinic1.DomainsList[0]}")
        });

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/allusers/clinics");
        request.Headers.Add(MultiTenantHeaders.CurrentTenantId, UserDataSeeding.Clinic1.Id.ToString());

        var response = await client.SendAsync(request);

        var responseString = await response.Content.ReadAsStringAsync();

        testOutputHelper.WriteLine(responseString);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var jobject = JArray.Parse(responseString);

        jobject.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetTenantMappings_ShouldReturnCorrectData()
    {
        var client = apiFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri($"http://{UserDataSeeding.Clinic1.DomainsList[0]}")
        });

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/allusers/tenantMappings");
        request.Headers.Add(MultiTenantHeaders.CurrentTenantId, UserDataSeeding.Clinic1.Id.ToString());

        var response = await client.SendAsync(request);

        var responseString = await response.Content.ReadAsStringAsync();

        testOutputHelper.WriteLine(responseString);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var jobject = JArray.Parse(responseString);
    }

    [Fact]
    public async Task GetUsersByTenant_ShouldReturnForbidden_WhenRequestingFromDifferentTenant()
    {
        var client = apiFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri($"http://{UserDataSeeding.Clinic1.DomainsList[0]}")
        });

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/allusers/users");

        request.Headers.Add(MultiTenantHeaders.CurrentTenantId, UserDataSeeding.Clinic2.Id.ToString());
        request.Headers.Add(CorsConstants.Origin, UserDataSeeding.Clinic1.WhitelistedOrigins);

        var response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}