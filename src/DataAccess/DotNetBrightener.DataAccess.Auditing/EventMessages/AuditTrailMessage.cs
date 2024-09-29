using DotNetBrightener.DataAccess.Auditing.Entities;
using DotNetBrightener.Plugins.EventPubSub;

namespace DotNetBrightener.DataAccess.Auditing.EventMessages;

public class AuditTrailMessage : DistributedEventMessage, ICombinationEventMessage
{
    public List<AuditEntity> AuditEntities { get; set; } = new();
}