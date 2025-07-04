using DotNetBrightener.OAuth.Models;

namespace DotNetBrightener.OAuth.Events;

public class ExternalAccountAuthorizedEvent
{
    public ExternalLoginData ExternalLogin { get; set; }

    public bool HasError { get; set; }

    public string ErrorMessage { get; set; }

    public Dictionary<string, object> ExtraData { get; set; }
}