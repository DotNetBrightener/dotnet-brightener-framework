using DotNetBrightener.DataAccess.Auditing.Entities;

namespace DotNetBrightener.DataAccess.Auditing.Internal;

internal class AuditEntriesContainer : IAuditEntriesContainer
{
    public List<AuditEntity> AuditEntries { get; } = new();
}