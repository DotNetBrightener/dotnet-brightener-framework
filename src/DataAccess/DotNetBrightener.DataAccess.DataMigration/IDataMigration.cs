namespace DotNetBrightener.DataAccess.DataMigration;

public interface IDataMigration
{
    Task MigrateData();
}