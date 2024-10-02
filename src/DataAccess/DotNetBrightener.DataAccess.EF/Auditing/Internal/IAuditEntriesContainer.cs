using DotNetBrightener.DataAccess.EF.Auditing;

namespace DotNetBrightener.DataAccess.Auditing.Internal;

internal interface IAuditEntriesContainer
{
    List<AuditEntity> AuditEntries { get; }
}