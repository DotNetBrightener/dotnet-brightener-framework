using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotNetBrightener.DataAccess.Models;

namespace DotNetBrightener.Infrastructure.AppClientManager.Models;

/// <summary>
///     Specifies the modules / scopes of the main application, that the client is allowed to access
/// </summary>
/// <remarks>
///     If a client is <b>granted</b> access to a module, but the user who logs in to the client app does not,
///     then it's expected that the user <b>is not</b> able to access the resource.<br />
///<br />
///     If the client is <b>not</b> granted access to a module, regardless of the user's permission,
///     the user who logs in to the client app still will not be able to access the module
/// </remarks>
public class AppClientAccessScope : BaseEntity
{
    /// <summary>
    ///     The identifier of the app client
    /// </summary>
    public long AppClientId { get; set; }

    /// <summary>
    ///     The scope or name/key of the permission that the client is granted access. Can be <c>null</c>.
    /// </summary>
    [MaxLength(512)]
    public string Scope { get; set; }

    /// <summary>
    ///     The destination of module or resource that the client is allowed to access. Can be <c>null</c>.
    /// </summary>
    [MaxLength(1000)]
    public string Destination { get; set; }

    /// <summary>
    ///     Describes the access scope
    /// </summary>
    [MaxLength(1000)]
    public string Description { get; set; }

    [ForeignKey(nameof(AppClientId))]
    public virtual AppClient AppClient { get; set; }
}