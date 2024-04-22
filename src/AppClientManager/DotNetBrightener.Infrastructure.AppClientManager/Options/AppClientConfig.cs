namespace DotNetBrightener.Infrastructure.AppClientManager.Options;

public class AppClientConfig
{
    public const string DefaultClientIdHeaderKey    = "X-Client-Id";
    public const string DefaultAppBundleIdHeaderKey = "X-App-Bundle-Id";
    public const string DefaultClientAccessTokenKey = "accessToken";

    public string ClientIdHeaderKey          { get; set; } = DefaultClientIdHeaderKey;

    public string ClientAppBundleIdHeaderKey { get; set; } = DefaultAppBundleIdHeaderKey;

    public string ClientAccessTokenKey { get; set; } = DefaultClientAccessTokenKey;

    /// <summary>
    ///     Specifies whether your application is open for public access.
    ///     If <c>true</c>, any client can access your app's resources and CORS will be enabled for all requests.
    /// </summary>
    public bool OpenForPublicAccess { get; set; } = false;
}