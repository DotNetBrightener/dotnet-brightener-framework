using DotNetBrightener.DataAccess.Models;
using System.ComponentModel.DataAnnotations;

namespace DotNetBrightener.MultiTenancy.Entities;

public class TenantEntityMapping: BaseEntity
{
    public Guid TenantId { get; set; }

    [MaxLength(1024)]
    public string EntityType { get; set; }

    public string EntityId { get; set; }
}