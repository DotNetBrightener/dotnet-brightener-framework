using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.EF.Repositories;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.Plugins.EventPubSub;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddEntityFrameworkDataServices<TDbContext>(this IServiceCollection serviceCollection,
                                                                  DatabaseConfiguration   dbConfiguration,
                                                                  Action<DbContextOptionsBuilder> configureAction =
                                                                      null)
        where TDbContext : DbContext
    {
        serviceCollection.AddSingleton(dbConfiguration);

        serviceCollection.AddDbContext<TDbContext>(configure =>
        {
            configureAction?.Invoke(configure);

            if (dbConfiguration.UseLazyLoading)
                configure.UseLazyLoadingProxies();
        });

        serviceCollection.AddScoped<DbContext, TDbContext>();

        serviceCollection.AddScoped<IRepository, Repository>();

        serviceCollection.TryAddScoped<IEventPublisher, EventPublisher>();

        serviceCollection.TryAddScoped<ITransactionWrapper, TransactionWrapper>();
        serviceCollection.TryAddScoped<ICurrentLoggedInUserResolver, DefaultCurrentUserResolver>();

        LinqToDBForEFTools.Initialize();
    }
}