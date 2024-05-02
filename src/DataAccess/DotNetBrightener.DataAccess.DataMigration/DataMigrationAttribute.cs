namespace DotNetBrightener.DataAccess.DataMigration;

[AttributeUsage(AttributeTargets.Class)]
public class DataMigrationAttribute(string migrationId) : Attribute
{
    public string MigrationId { get; } = migrationId;
}