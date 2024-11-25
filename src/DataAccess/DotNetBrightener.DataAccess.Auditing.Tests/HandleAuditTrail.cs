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
        foreach (var auditEntity in eventMessage.AuditEntities)
        {
            logger.LogDebug(Environment.NewLine +
                            "[{appVersion}] Entity was {state}. Changed properties:\r\n{@changeProperties}\r\n",
                            auditEntity.AuditToolVersion,
                            auditEntity.Action,
                            JsonConvert.SerializeObject(auditEntity.AuditProperties, Formatting.Indented));

            mocker.ChangedProperties(auditEntity.AuditProperties);
        }

        return true;
    }

    public int Priority => 10_000;
}