namespace DotNetBrightener.DataAccess.DataMigration.Extensions;

internal class DataMigrationMetadata : Dictionary<string, Type>
{
    public IDataMigration GetMigration(IServiceProvider scopedServiceProvider, string migrationId)
    {
        if (TryGetValue(migrationId, out var type))
        {
            return (IDataMigration)scopedServiceProvider.TryGet(type);
        }

        return null;
    }
}