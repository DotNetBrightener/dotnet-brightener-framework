using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DotNetBrightener.DataAccess.Auditing.Auditing;


public class AuditProperty
{
    public string PropertyName { get; set; }

    public object OldValue { get; set; }

    public object NewValue { get; set; }
}

public class AuditTrail<T>
{
    public string Identifier { get; set; }

    public List<AuditProperty> AuditProperties { get; set; } = new List<AuditProperty>();
}

public class AuditEntity
{
    [Key]
    public long Id { get; set; }

    public string EntityType { get; set; }

    public string EntityIdentifier { get; set; }

    public string Changes { get; set; }

    public DateTimeOffset? Timestamp { get; set; }

    public string UserName { get; set; }
}