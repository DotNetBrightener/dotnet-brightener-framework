using DotNetBrightener.Core.Logging;
using DotNetBrightener.Core.Logging.DbStorage;
using DotNetBrightener.Core.Logging.DbStorage.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class LoggingStorageServiceCollectionExtensions
{
    public static IServiceCollection AddLogSqlServerStorage(this IServiceCollection serviceCollection,
                                                            string                  connectionString)
    {
        serviceCollection.Replace(
                                  ServiceDescriptor
                                     .Scoped<IQueueEventLogBackgroundProcessing, QueueEventLogBackgroundProcessing>());

        serviceCollection.AddDbContext<LoggingDbContext>((optionBuilder) =>
        {
            optionBuilder.UseSqlServer(connectionString,
                                       x => x.MigrationsHistoryTable("__MigrationsHistory",
                                                                     LoggingDbContext.SchemaName));

            optionBuilder.UseLazyLoadingProxies();
        });

        return serviceCollection;
    }
}