﻿using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.EF.Migrations;
using DotNetBrightener.DataAccess.EF.Options;
using DotNetBrightener.DataAccess.EF.Repositories;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.Plugins.EventPubSub;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkDataServices<TDbContext>(
        this IServiceCollection serviceCollection,
        DatabaseConfiguration   dbConfiguration,
        IConfiguration          configuration,
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
        serviceCollection.TryAddScoped<IEventPublisher, EventPublisher>();
        serviceCollection.TryAddScoped<ICurrentLoggedInUserResolver, DefaultCurrentUserResolver>();

        serviceCollection.Configure<DataMigrationOptions>(configuration.GetSection(nameof(DataMigrationOptions)));

        LinqToDBForEFTools.Initialize();

        return serviceCollection;
    }

    /// <summary>
    ///     Add the migration <typeparamref name="TMigrationDbContext"/> to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    ///     <typeparamref name="TMigrationDbContext"/> is derived from <typeparamref name="TBaseDbContext"/>
    ///     and only used for migrations
    /// </remarks>
    /// <typeparam name="TMigrationDbContext">
    ///     The <see cref="DbContext" /> derived from <seealso cref="TBaseDbContext"/> and only used for migrations
    /// </typeparam>
    /// <typeparam name="TBaseDbContext">
    ///     The <see cref="DbContext" /> that contains all the entities,
    ///     which is the base for <seealso cref="TMigrationDbContext"/>
    /// </typeparam>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection" /> to add the migration <typeparamref name="TMigrationDbContext"/>
    /// </param>
    public static IServiceCollection UseMigrationDbContext<TMigrationDbContext, TBaseDbContext>(
        this IServiceCollection serviceCollection)
        where TMigrationDbContext : TBaseDbContext, IMigrationDefinitionDbContext<TBaseDbContext>
        where TBaseDbContext : DbContext
    {
        serviceCollection.AddDbContext<TMigrationDbContext>();
        serviceCollection.AddHostedService<AutoMigrateDbStartupTask<TMigrationDbContext>>();

        return serviceCollection;
    }
}