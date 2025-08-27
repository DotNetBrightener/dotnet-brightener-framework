using DotNetBrightener.Utils.MessageCompression;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Net;
using Xunit;

namespace DotNetBrightener.SecuredApi.Tests;

public class SecureApiTest_RegisterHandlerViaMapping : IAsyncDisposable
{
    private WebApplication _host;
    private int            _port;

    public SecureApiTest_RegisterHandlerViaMapping()
    {
        // Arrange
        _port    = new Random().Next(32454, 33000);
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddSecuredApi();

        builder.WebHost.UseUrls($"http://localhost:{_port}");

        _host = builder.Build();

        _host.UseSecureApiHandle();
        _host.MapSecuredPost<SyncUserService>("test/syncUser");

        // Acts
        _host.StartAsync().Wait();
    }

    public async ValueTask DisposeAsync()
    {
        await TearDownHost();
    }

    [Theory]
    [InlineData("/test/syncUser", HttpStatusCode.OK, "POST")]
    [InlineData("/test/syncUser", HttpStatusCode.NotFound, "PUT")]
    [InlineData("/test/syncUser", HttpStatusCode.NotFound, "GET")]
    [InlineData("/test/sync-user", HttpStatusCode.NotFound, "GET")]
    [InlineData("/test/this-action-is-not-found", HttpStatusCode.NotFound, "GET")]
    [InlineData("/test?action=syncUser", HttpStatusCode.NotFound, "GET")]
    [InlineData("/test?action=sync-user", HttpStatusCode.NotFound, "GET")]
    [InlineData("/should-be-not-found", HttpStatusCode.NotFound, "GET")]
    public async Task RequestToSecuredApi_ShouldSuccess(string requestUrl,
                                                        HttpStatusCode expectedResponseCode,
                                                        string httpMethod)
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

        response.StatusCode.ShouldBe(expectedResponseCode);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            UserRecord? responseData = null;

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await responseStream.CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    responseData = await ms.Decompress<UserRecord>(1024 * 4);
                }
            }

            responseData.ShouldNotBeNull();
            responseData.Id.ShouldBe(5100);
            responseData.Name.ShouldBe("test user");
        }
    }

    private async Task TearDownHost()
    {
        await _host.StopAsync();
        await _host.DisposeAsync();
    }
}