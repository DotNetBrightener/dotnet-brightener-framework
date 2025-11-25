#nullable enable
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Immutable;
using System.Reflection;

namespace DotNetBrightener.DataAccess.EF.Auditing;

internal interface IAuditEntriesProcessor
{
    Task QueueAuditEntries(params AuditEntity[] auditEntities);
}

internal class AuditEntriesProcessor(IServiceScopeFactory serviceScopeFactory, 
                                     ILogger<AuditEntriesProcessor> logger) : IAuditEntriesProcessor
{
    private readonly List<AuditEntity> _auditEntitiesQueue = [];
    private readonly TimeSpan          _delay              = TimeSpan.FromSeconds(5);
    private          Timer             _timer;
    private readonly Lock              _lock = new();

    private static readonly AssemblyInformationalVersionAttribute VersionInfo = Assembly.GetExecutingAssembly()
                                                                                        .GetCustomAttribute<AssemblyInformationalVersionAttribute>();

    private string? VersionString => VersionInfo?.InformationalVersion.Split("+").FirstOrDefault();

    public async Task QueueAuditEntries(params AuditEntity[] auditEntities)
    {
        lock (_lock)
        {
            _auditEntitiesQueue.AddRange(auditEntities);

            if (_timer == null)
            {
                _timer = new Timer(ProcessAuditEntries, null, _delay, Timeout.InfiniteTimeSpan);
                logger.LogInformation("Initialized timer with {delay} delay before processing audit entities.", _delay);
            }
            else
            {
                logger.LogInformation("New entities added. Postponing {delay} before processing audit entities.", _delay);
                _timer.Change(_delay, Timeout.InfiniteTimeSpan);
            }
        }
    }

    private async void ProcessAuditEntries(object state)
    {
        ImmutableList<AuditEntity> auditEntitiesToProcess;

        lock (_lock)
        {
            auditEntitiesToProcess = [.._auditEntitiesQueue.DistinctBy(x => x.Id)];
            _auditEntitiesQueue.Clear();

            foreach (var auditEntry in auditEntitiesToProcess)
            {
                auditEntry.AuditToolVersion = VersionString;
                auditEntry.Changes          = JsonConvert.SerializeObject(auditEntry.AuditProperties);
            }

            _timer.Dispose();
            _timer = null;

            logger.LogInformation("Cleaned up timers and audit queue. Have {auditEntriesCount} entries to process.",
                                  auditEntitiesToProcess.Count);
        }

        using (var scope = serviceScopeFactory.CreateScope())
        {
            var eventPublisher = scope.ServiceProvider.TryGet<IEventPublisher>();

            if (eventPublisher is not null)
            {
                logger.LogInformation("Forwarding {auditEntriesCount} entries to handler services.",
                                      auditEntitiesToProcess.Count);

                _ = eventPublisher.Publish(new AuditTrailMessage
                                           {
                                               AuditEntities = auditEntitiesToProcess
                                           },
                                           true);
            }
        }
    }
}