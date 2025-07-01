#nullable enable
using System.ComponentModel.DataAnnotations;
using DotNetBrightener.DataAccess.Models;

namespace DotNetBrightener.MultiTenancy.Entities;

public abstract class TenantBase : GuidBaseEntityWithAuditInfo
{
    /// <summary>
    ///     The name of the tenant
    /// </summary>
    [MaxLength(1024)]
    public string Name { get; set; }

    /// <summary>
    ///     The domains where the tenant is hosted
    /// </summary>
    [MaxLength(1024)]
    public string TenantDomains { get; set; } = "";

    /// <summary>
    ///     The origins that are whitelisted to access the tenant's resources
    /// </summary>
    [MaxLength(10240)]
    public string WhitelistedOrigins { get; set; } = "";

    public List<string> DomainsList => TenantDomains
                                      .Split([";"],
                                             StringSplitOptions.RemoveEmptyEntries |
                                             StringSplitOptions.TrimEntries)
                                      .ToList();

    public List<string> WhitelistedOriginsList => WhitelistedOrigins
                                                 .Split([";"],
                                                        StringSplitOptions.RemoveEmptyEntries |
                                                        StringSplitOptions.TrimEntries)
                                                 .ToList();

    public void AddWhitelistedOrigins(params List<string> origins)
    {
        var existingOrigins = WhitelistedOrigins.Split([
                                                           ";"
                                                       ],
                                                       StringSplitOptions.RemoveEmptyEntries)
                                                .ToList();

        existingOrigins.AddRange(origins);

        existingOrigins = existingOrigins.Distinct()
                                         .ToList();

        WhitelistedOrigins = string.Join(';', existingOrigins);
    }
}