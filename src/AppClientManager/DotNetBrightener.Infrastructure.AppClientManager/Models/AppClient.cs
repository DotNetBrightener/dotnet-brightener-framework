using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DotNetBrightener.DataAccess.Models;

namespace DotNetBrightener.Infrastructure.AppClientManager.Models;

/// <summary>
///     Represents an App Client
/// </summary>
public class AppClient : BaseEntityWithAuditInfo
{
    /// <summary>
    ///     The name of the client
    /// </summary>
    [MaxLength(100)]
    public string ClientName { get; set; }

    /// <summary>
    ///     Identifier of the client
    /// </summary>
    [MaxLength(64)]
    public string ClientId { get; set; }

    /// <summary>
    ///     Type of the client, e.g. Web, Mobile, Desktop
    /// </summary>
    public AppClientType ClientType { get; set; }

    /// <summary>
    ///     The status of the client
    /// </summary>
    public AppClientStatus ClientStatus { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [JsonIgnore]
    [MaxLength(5000)]
    public string? ClientSecretHashed { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [JsonIgnore]
    [MaxLength(5000)]
    public string? ClientSecretSalt { get; set; }

    /// <summary>
    ///     Description for the app client
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    ///     Specifies the client app host names, if <see cref="ClientType"/> is <b><see cref="AppClientType.Web"/></b>,
    ///     that are allowed to access the resource
    /// </summary>
    [MaxLength(2000)]
    public string? AllowedOrigins { get; set; }

    /// <summary>
    ///     Specifies the client app bundle ids,
    ///     if <see cref="ClientType"/> is <b><see cref="AppClientType.Mobile"/></b> or <b><see cref="AppClientType.Desktop"/></b>,
    ///     that are allowed to access the resource
    /// </summary>
    [MaxLength(2000)]
    public string? AllowedAppBundleIds { get; set; }

    /// <summary>
    ///     Describes the reason if the app is deactivated
    /// </summary>
    [MaxLength(2000)]
    public string? DeactivatedReason { get; set; }
}