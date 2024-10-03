using System.Collections.Immutable;
using DotNetBrightener.Plugins.EventPubSub;

namespace DotNetBrightener.DataAccess.EF.Auditing;

public class AuditTrailMessage : INonStoppedEventMessage
{
    public ImmutableList<AuditEntity> AuditEntities { get; init; }
}