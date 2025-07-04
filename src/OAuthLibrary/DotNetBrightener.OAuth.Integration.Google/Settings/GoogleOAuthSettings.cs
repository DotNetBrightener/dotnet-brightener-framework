using DotNetBrightener.OAuth.Settings;

namespace DotNetBrightener.OAuth.Integration.Google.Settings;

public class GoogleOAuthSettings: IOAuthProviderSetting
{
    public string ClientId { get; set; }

    public string ClientSecret { get; set; }
}