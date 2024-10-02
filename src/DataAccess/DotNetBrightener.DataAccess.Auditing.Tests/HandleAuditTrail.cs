using DotNetBrightener.DataAccess.EF.Auditing;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DotNetBrightener.DataAccess.Auditing.Tests;

public class HandleAuditTrail(IMockReceiveData mocker, 
                              ILogger<HandleAuditTrail> logger) : IEventHandler<AuditTrailMessage>
{
    public async Task<bool> HandleEvent(AuditTrailMessage eventMessage)
    {
        mocker.ReceiveData(eventMessage);

        var auditEntries = eventMessage.AuditEntities;

        foreach (var auditEntity in auditEntries)
        {
            mocker.ChangedProperties(auditEntity.AuditProperties);

            logger.LogDebug("Entity was {state}. Changed properties: {@changeProperties}",
                            auditEntity.Action,
                            JsonConvert.SerializeObject(auditEntity.AuditProperties, Formatting.Indented));
        }

        return true;
    }

    public int Priority => 10_000;
}