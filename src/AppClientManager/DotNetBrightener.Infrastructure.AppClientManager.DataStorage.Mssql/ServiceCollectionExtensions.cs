using DotNetBrightener.Infrastructure.AppClientManager;
using DotNetBrightener.Infrastructure.AppClientManager.DataStorage;
using DotNetBrightener.Infrastructure.AppClientManager.DataStorage.Mssql;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable CheckNamespace

namespace Microsoft.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
    public static AppClientManagerBuilder UseSqlServer(this AppClientManagerBuilder appClientManagerBuilder,
                                                       string                       connectionString)
    {
        var services = appClientManagerBuilder.Services;

        services.UseDbContextWithMigration<AppClientDbContext, SqlServerMigrationDbContext>((serviceProvider,
                                                                                             options) =>
        {
            options.UseSqlServer(connectionString,
                                 x => x.MigrationsHistoryTable("__MigrationsHistory",
                                                               AppClientDataDefaults
                                                                  .AppClientSchemaName));
        });

        return appClientManagerBuilder;
    }

    public static AppClientManagerBuilder UseSqlServer(this AppClientManagerBuilder appClientManagerBuilder,
                                                       Func<IServiceProvider, DbContextOptionsBuilder, string>
                                                           connectionStringResolver)
    {
        var services = appClientManagerBuilder.Services;

        services.UseDbContextWithMigration<AppClientDbContext, SqlServerMigrationDbContext>((serviceProvider,
                                                                                             options) =>
        {
            var connectionString = connectionStringResolver.Invoke(serviceProvider, options);
            options.UseSqlServer(connectionString,
                                 x => x.MigrationsHistoryTable("__MigrationsHistory",
                                                               AppClientDataDefaults
                                                                  .AppClientSchemaName));
        });

        return appClientManagerBuilder;
    }
}