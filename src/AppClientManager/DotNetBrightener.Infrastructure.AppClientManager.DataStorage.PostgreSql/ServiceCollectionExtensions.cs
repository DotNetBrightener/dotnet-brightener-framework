using DotNetBrightener.Infrastructure.AppClientManager;
using DotNetBrightener.Infrastructure.AppClientManager.DataStorage;
using DotNetBrightener.Infrastructure.AppClientManager.DataStorage.PostgreSql;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable CheckNamespace
namespace Microsoft.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
    public static AppClientManagerBuilder UsePostgreSql(this AppClientManagerBuilder appClientManagerBuilder,
                                                       string                       connectionString)
    {
        var services = appClientManagerBuilder.Services;
        services.UseDbContextWithMigration<AppClientDbContext, MigrationPostgreSqlDbContext>(option =>
        {
            option.UseNpgsql(connectionString,
                             x => x.MigrationsHistoryTable("__MigrationsHistory",
                                                           AppClientDataDefaults
                                                              .AppClientSchemaName));
        });

        return appClientManagerBuilder;
    }
}