namespace DotNetBrightener.DataAccess.EF.Auditing.Internal;

internal interface IAuditEntriesContainer
{
    List<AuditEntity> AuditEntries { get; }
}