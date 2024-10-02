using DotNetBrightener.DataAccess.Auditing.Entities;
using DotNetBrightener.Plugins.EventPubSub;

namespace DotNetBrightener.DataAccess.EF.Auditing;

public class AuditTrailMessage : INonStoppedEventMessage
{
    public List<AuditEntity> AuditEntities { get; set; } = new();
}