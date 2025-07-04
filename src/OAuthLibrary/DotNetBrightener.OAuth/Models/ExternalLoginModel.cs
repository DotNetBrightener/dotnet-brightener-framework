namespace DotNetBrightener.OAuth.Models;

public class ExternalLoginModel
{
    public long?  Id           { get; set; }
    public long?  UserId       { get; set; }
    public string ProviderName { get; set; }
    public string ExternalId   { get; set; }
}