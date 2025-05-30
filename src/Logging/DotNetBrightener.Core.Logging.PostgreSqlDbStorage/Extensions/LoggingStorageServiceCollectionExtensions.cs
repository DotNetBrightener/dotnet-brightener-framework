using DotNetBrightener.Core.Logging;
using DotNetBrightener.Core.Logging.PostgreSqlDbStorage;
using DotNetBrightener.Core.Logging.PostgreSqlDbStorage.Data;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class LoggingStorageServiceCollectionExtensions
{
    public static IServiceCollection AddLogPostgreSqlStorage(this IServiceCollection serviceCollection,
                                                             string                  connectionString)
    {
        serviceCollection.Replace(
                                  ServiceDescriptor
                                     .Scoped<IQueueEventLogBackgroundProcessing,
                                          NpgsqlQueueEventLogBackgroundProcessing>());

        serviceCollection.AddDbContext<LoggingDbContext>((optionBuilder) =>
        {
            optionBuilder.UseNpgsql(connectionString,
                                    x => x.MigrationsHistoryTable("__MigrationsHistory",
                                                                  LoggingDbContext.SchemaName));

            optionBuilder.UseLazyLoadingProxies();
        });

        LinqToDBForEFTools.Initialize();

        return serviceCollection;
    }
}