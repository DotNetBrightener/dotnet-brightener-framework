using DotNetBrightener.Core.Logging;
using DotNetBrightener.Core.Logging.DbStorage;
using DotNetBrightener.Core.Logging.DbStorage.Data;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class LoggingStorageServiceCollectionExtensions
{
    public static IServiceCollection AddLogStorage(this IServiceCollection         serviceCollection,
                                                   Action<DbContextOptionsBuilder> buildOption)
    {
        serviceCollection.Replace(
                                  ServiceDescriptor
                                     .Scoped<IQueueEventLogBackgroundProcessing, QueueEventLogBackgroundProcessing>());

        serviceCollection.AddDbContext<LoggingDbContext>((optionBuilder) =>
        {
            buildOption(optionBuilder);

            optionBuilder.UseLazyLoadingProxies();
        });

        serviceCollection.AddHostedService<MigrateLoggingContextHostedService>();

        LinqToDBForEFTools.Initialize();

        return serviceCollection;
    }
}
