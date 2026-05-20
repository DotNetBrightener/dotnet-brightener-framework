using DotNetBrightener.Utils.MessageCompression;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Net;
using Xunit;

namespace DotNetBrightener.SecuredApi.Tests;

public class SecureApiTest_RegisterHandlerViaMappingGroupedEndpoint : IAsyncLifetime
{
    private WebApplication? _host;
    private HttpClient?     _httpClient;

    public async Task InitializeAsync()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddSecuredApi();

        builder.WebHost.UseTestServer();

        _host = builder.Build();

        _host.UseSecureApiHandle("test");
        _host.MapSecuredPost<SyncUserService>("syncUser");

        // Acts
        await _host.StartAsync();
        _httpClient = _host.GetTestClient();
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
    public async Task RequestToSecuredApi_ShouldSuccess(string         requestUrl,
                                                        HttpStatusCode expectedResponseCode,
                                                        string         httpMethod)
    {
        var method = HttpMethod.Parse(httpMethod);

        var apiMessagePayload = new UserRecord();

        var apiMessage = ApiMessage.FromPayload(apiMessagePayload);

        var request           = await apiMessage.ToJsonBytes();
        var compressedRequest = await request.Compress();

        var requestMsg = new HttpRequestMessage(method, requestUrl)
        {
            Content = new ByteArrayContent(compressedRequest)
        };

        var response = await _httpClient!.SendAsync(requestMsg);

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

    public async Task DisposeAsync()
    {
        _httpClient?.Dispose();

        if (_host is not null)
        {
            await _host.StopAsync();
            await _host.DisposeAsync().AsTask();
        }
    }
}
