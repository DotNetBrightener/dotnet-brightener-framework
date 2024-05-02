using DotNetBrightener.DataAccess.DataMigration;
using DotNetBrightener.DataAccess.DataMigration.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DataMigrationsServiceCollectionExtensions
{
    public static DataMigrationConfiguration UseSqlServer(this DataMigrationConfiguration configuration,
                                                          string                          connectionString)
    {
        configuration.ConnectionString = connectionString;
        configuration.ServiceCollection.AddDbContext<DataMigrationDbContext>(options =>
        {
            options.UseSqlServer(connectionString,
                                 x =>
                                 {
                                     x.MigrationsHistoryTable(configuration.MigrationHistoryTableName,
                                                              configuration.MigrationHistoryTableSchema);

                                     x.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                                 });
        });

        return configuration;
    }
}