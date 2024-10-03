using DotNetBrightener.DataAccess.Models.Auditing;
using System.Collections.Immutable;

namespace DotNetBrightener.DataAccess.Auditing.Tests;

public interface IMockReceiveData
{
    void ChangedProperties(ImmutableList<AuditProperty> auditEntityChangedAuditProperties);
}