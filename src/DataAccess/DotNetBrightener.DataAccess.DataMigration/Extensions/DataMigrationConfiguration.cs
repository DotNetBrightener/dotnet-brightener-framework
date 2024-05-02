using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.DataAccess.DataMigration.Extensions;

public class DataMigrationConfiguration
{
    public string ConnectionString { get; set; }

    internal string MigrationHistoryTableName { get; } = "__SchemaMigrationsHistory";

    internal string             MigrationHistoryTableSchema { get; } = DataMigrationDbContext.SchemaName;

    internal IServiceCollection ServiceCollection           { get; set; }
}