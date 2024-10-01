using DotNetBrightener.DataAccess.Auditing.Entities;

namespace DotNetBrightener.DataAccess.Auditing.Internal;

internal interface IAuditEntriesContainer
{
    List<AuditEntity> AuditEntries { get; }
}