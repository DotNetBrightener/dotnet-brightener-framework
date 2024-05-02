namespace DotNetBrightener.DataAccess.DataMigration.Extensions;

[AttributeUsage(AttributeTargets.Class)]
public class DataMigrationAttribute(string migrationId) : Attribute
{
    public string MigrationId { get; } = migrationId;
}