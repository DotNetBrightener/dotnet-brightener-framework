using DotNetBrightener.DataAccess.EF.Auditing;

namespace DotNetBrightener.DataAccess.Auditing.Internal;

internal class AuditEntriesContainer : IAuditEntriesContainer
{
    public List<AuditEntity> AuditEntries { get; } = new();
}