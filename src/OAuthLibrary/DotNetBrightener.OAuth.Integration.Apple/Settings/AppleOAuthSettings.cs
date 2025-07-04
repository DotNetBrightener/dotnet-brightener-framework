using DotNetBrightener.OAuth.Settings;

namespace DotNetBrightener.OAuth.Integration.Apple.Settings;

public class AppleOAuthSettings: IOAuthProviderSetting
{
    public string ClientId { get; set; }
    public string MobileClientId { get; set; }

    public string PrivateKey { get; set; }
    
    public string TeamId { get; set; }
    
    public string KeyId { get; set; }
}