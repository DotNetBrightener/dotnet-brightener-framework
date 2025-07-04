namespace DotNetBrightener.OAuth.Models;

public class ExternalLoginData
{
    public long?                     LinkedUserId           { get; set; }
    public string                    ProviderName           { get; set; }
    public string                    ExternalId             { get; set; }
    public string                    UserName               { get; set; }
    public string                    ExternalAccessToken    { get; set; }
    public string                    ProfileImageUrl        { get; set; }
    public string                    ProfileImageUrlCropped { get; set; }
    public string                    Location               { get; set; }
    public string                    FirstName              { get; set; }
    public string                    LastName               { get; set; }
    public Dictionary<string,object> ExtraParameters        { get; set; }
}