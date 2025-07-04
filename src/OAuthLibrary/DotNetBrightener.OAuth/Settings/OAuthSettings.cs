

namespace DotNetBrightener.OAuth.Settings;

public class OAuthSettings
{
    public bool ForceHttps { get; set; }

    public string EnabledOrigins { get; set; }

    /// <summary>
    /// Default redirect URL for mobile clients when none is provided
    /// </summary>
    public string DefaultMobileRedirectUrl { get; set; }

    /// <summary>
    /// Default redirect URL for web clients when Referer header is not available
    /// </summary>
    public string DefaultWebRedirectUrl { get; set; }
}
