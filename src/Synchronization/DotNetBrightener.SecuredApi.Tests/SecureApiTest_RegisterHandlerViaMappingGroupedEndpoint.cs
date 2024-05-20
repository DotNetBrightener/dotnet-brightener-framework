using DotNetBrightener.Utils.MessageCompression;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Net;

namespace DotNetBrightener.SecuredApi.Tests;

internal class SecureApiTest_RegisterHandlerViaMappingGroupedEndpoint
{
    private WebApplication _host;
    private int            _port;

    [SetUp]
    public async Task Setup()
    {
        // Arrange
        _port = new Random().Next(32454, 33000);
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddSecuredApi();

        builder.WebHost.UseUrls($"http://localhost:{_port}");

        _host = builder.Build();

        _host.UseSecureApiHandle("test");
        _host.MapSecuredPost<SyncUserService>("syncUser");

        // Acts
        await _host.StartAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await TearDownHost();
    }

    [Test]
    [TestCase("/test/syncUser", HttpStatusCode.OK, "POST")]
    [TestCase("/test/syncUser", HttpStatusCode.NotFound, "PUT")]
    [TestCase("/test/syncUser", HttpStatusCode.NotFound, "GET")]
    [TestCase("/test/sync-user", HttpStatusCode.NotFound)]
    [TestCase("/test/this-action-is-not-found", HttpStatusCode.NotFound)]
    [TestCase("/test?action=syncUser", HttpStatusCode.NotFound)]
    [TestCase("/test?action=sync-user", HttpStatusCode.NotFound)]
    [TestCase("/should-be-not-found", HttpStatusCode.NotFound)]
    public async Task RequestToSecuredApi_ShouldSuccess(string         requestUrl,
                                                        HttpStatusCode expectedResponseCode,
                                                        string         httpMethod = "GET")
    {
        var method = HttpMethod.Parse(httpMethod);

        var httpClient = new HttpClient();

        var apiMessagePayload = new UserRecord();

        var apiMessage = ApiMessage.FromPayload(apiMessagePayload);

        var request           = await apiMessage.ToJsonBytes();
        var compressedRequest = await request.Compress();

        var requestMsg = new HttpRequestMessage(method, $"http://localhost:{_port}{requestUrl}")
        {
            Content = new ByteArrayContent(compressedRequest)
        };

        var response = await httpClient.SendAsync(requestMsg);

        Assert.That(response.StatusCode, Is.EqualTo(expectedResponseCode));

        if (response.StatusCode == HttpStatusCode.OK)
        {
            UserRecord responseData = null;

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await responseStream.CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    responseData = await ms.Decompress<UserRecord>(1024 * 4);
                }
            }

            Assert.That(responseData, Is.Not.Null);
            Assert.That(responseData.Id, Is.EqualTo(5100));
            Assert.That(responseData.Name, Is.EqualTo("test user"));
        }
    }

    private async Task TearDownHost()
    {
        await _host.StopAsync();
        await _host.DisposeAsync();
    }
}