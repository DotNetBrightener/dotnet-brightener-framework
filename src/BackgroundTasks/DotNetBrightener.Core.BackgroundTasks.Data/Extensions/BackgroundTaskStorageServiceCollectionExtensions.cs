using DotNetBrightener.Core.BackgroundTasks.Data;
using DotNetBrightener.Core.BackgroundTasks.Data.DbContexts;
using Microsoft.EntityFrameworkCore;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class BackgroundTaskStorageServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundTaskStorage(this IServiceCollection         serviceCollection,
                                                              Action<DbContextOptionsBuilder> buildOption)
    {
        serviceCollection.AddDbContext<BackgroundTaskDbContext>((optionBuilder) =>
        {
            buildOption(optionBuilder);

            optionBuilder.UseLazyLoadingProxies();
        });

        serviceCollection.AddHostedService<MigrateBackgroundTaskDbContextHostedService>();

        return serviceCollection;
    }
}