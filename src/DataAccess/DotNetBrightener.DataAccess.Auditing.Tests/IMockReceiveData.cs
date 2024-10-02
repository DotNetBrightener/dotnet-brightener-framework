using System.Collections.Immutable;
using DotNetBrightener.DataAccess.EF.Auditing;
using DotNetBrightener.DataAccess.Models.Auditing;

namespace DotNetBrightener.DataAccess.Auditing.Tests;

public interface IMockReceiveData
{
    void ReceiveData(AuditTrailMessage                  data);

    void ChangedProperties(ImmutableList<AuditProperty> auditEntityChangedAuditProperties);
}