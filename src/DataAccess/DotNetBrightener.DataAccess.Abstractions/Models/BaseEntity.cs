using System;
using System.ComponentModel.DataAnnotations;

namespace DotNetBrightener.DataAccess.Models;

public abstract class BaseEntity
{
    [Key]
    public long Id { get; set; }
}

public abstract class BaseEntityWithAuditInfo : BaseEntity
{
    public bool IsDeleted { get; set; }

    public DateTimeOffset? CreatedDate { get; set; }

    public string CreatedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }

    public string ModifiedBy { get; set; }
}