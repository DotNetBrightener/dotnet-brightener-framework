namespace DotNetBrightener.PushNotification.APN;

public class ApnSettings
{
    public string TeamId { get; set; }

    public string KeyId { get; set; }

    public string PrivateKey { get; set; }

    public string BundleId { get; set; }

    public bool UseSandbox { get; set; } = true;

    public string ApnServerUrl => UseSandbox 
        ? "https://api.sandbox.push.apple.com" 
        : "https://api.push.apple.com";
}
