﻿using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace DotNetBrightener.DataAccess.Models.Auditing;

public class AuditProperty
{
    public string PropertyName { get; set; }

    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public object OldValue { get; set; }

    public object NewValue { get; set; }
}

public class AuditTrail
{
    public string Identifier { get; set; }

    public string Action { get; set; }

    public List<AuditProperty> AuditProperties { get; set; } = new List<AuditProperty>();
}

public class AuditTrail<T>: AuditTrail
{
    public string EntityName { get; set; } = typeof(T).Name;

    public string EntityFullName { get; set; } = typeof(T).FullName;
}
