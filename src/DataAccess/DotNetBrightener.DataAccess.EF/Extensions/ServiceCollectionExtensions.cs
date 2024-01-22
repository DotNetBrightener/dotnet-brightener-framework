using DotNetBrightener.DataAccess.EF.Repositories;
using DotNetBrightener.DataAccess.Services;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using DotNetBrightener.DataAccess.EF.Auditing;
using DotNetBrightener.Plugins.EventPubSub;

namespace DotNetBrightener.DataAccess.EF.Extensions;

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

        serviceCollection.AddScoped<IAuditingContainer, AuditingContainer>();

        serviceCollection.TryAddScoped<ICurrentLoggedInUserResolver, DefaultCurrentUserResolver>();

        LinqToDBForEFTools.Initialize();
    }
}