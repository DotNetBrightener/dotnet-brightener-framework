using System.ComponentModel.DataAnnotations;
using DotNetBrightener.DataAccess.Models;

namespace DotNetBrightener.MultiTenancy.Entities;

public class Tenant : BaseEntity
{
    [MaxLength(1024)]
    public string TenantName { get; set; }

    [MaxLength(40)]
    public string TenantGuid { get; set; } = Guid.CreateVersion7().ToString();

    [MaxLength(10240)]
    public string TenantDomains { get; set; }

    public string PublicSiteUrl { get; set; }

    public string AdminSiteUrl { get; set; }

    public List<string> Domains => TenantDomains
                                  .Split([";"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                  .ToList();

    public void SetTenantDomains(params string[] domains)
    {
        TenantDomains = string.Join(";", domains) + ";";
    }
}