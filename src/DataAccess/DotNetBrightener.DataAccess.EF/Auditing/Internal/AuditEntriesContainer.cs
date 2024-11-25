namespace DotNetBrightener.DataAccess.EF.Auditing.Internal;

internal class AuditEntriesContainer : IAuditEntriesContainer
{
    public List<AuditEntity> AuditEntries { get; } = new();
}