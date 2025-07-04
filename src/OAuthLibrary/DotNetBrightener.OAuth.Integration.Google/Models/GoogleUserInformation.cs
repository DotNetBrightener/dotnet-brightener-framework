using Newtonsoft.Json;

namespace DotNetBrightener.OAuth.Integration.Google.Models;

public class GoogleUserInformation
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("family_name")]
    public string FamilyName { get; set; }
    [JsonProperty("given_name")]
    public string GivenName { get; set; }
    [JsonProperty("email")]
    public string Email { get; set; }
    [JsonProperty("picture")]
    public string Photo { get; set; }
    [JsonProperty("verified_email")]
    public bool VerifiedEmail { get; set; }
}


public class GoogleUserInformationClientModel
{
    public string Photo      { get; set; }
    public string GivenName  { get; set; }
    public string FamilyName { get; set; }
    public string Name       { get; set; }
    public string Email      { get; set; }
    public string Id         { get; set; }
}