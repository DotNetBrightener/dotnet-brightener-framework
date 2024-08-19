using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.EF.Migrations;
using DotNetBrightener.DataAccess.EF.Options;
using DotNetBrightener.DataAccess.EF.Repositories;
using DotNetBrightener.DataAccess.Services;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public class EfDataServiceConfigurator
{
    internal IServiceCollection              ServiceCollection            { get; set; }
    internal Action<DbContextOptionsBuilder> SharedDbContextOptionBuilder { get; set; }
    internal DatabaseConfiguration           DbConfiguration              { get; set; }
}

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Register services needed for enabling centralized data service using Entity Framework,
    ///     and adds the <typeparamref name="TDbContext"/> as the centralized <see cref="DbContext"/> for the application.
    /// </summary>
    /// <typeparam name="TDbContext">
    ///     The type of the <see cref="DbContext"/>
    /// </typeparam>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection"/> to register the services
    /// </param>
    /// <param name="dbConfiguration">
    ///     The centralized <see cref="DatabaseConfiguration"/> applied to the entire application
    /// </param>
    /// <param name="configuration">
    ///     The <see cref="IConfiguration"/> to obtain configuration settings
    /// </param>
    /// <returns>
    ///     The <see cref="EfDataServiceConfigurator"/> for chaining operations
    /// </returns>
    public static EfDataServiceConfigurator AddEFCentralizedDataServices<TDbContext>(
        this IServiceCollection         serviceCollection,
        DatabaseConfiguration           dbConfiguration,
        IConfiguration                  configuration,
        Action<DbContextOptionsBuilder> configureAction)
        where TDbContext : DbContext
    {
        if (configureAction is null)
            throw new ArgumentNullException(nameof(configureAction), "configureAction must be provided");

        var configurator = serviceCollection.AddEFCentralizedDataServices(dbConfiguration, configuration);

        configurator.WithCentralizeDbContext<TDbContext>(configureAction);

        return configurator;
    }

    /// <summary>
    ///     Register services needed for enabling centralized data service using Entity Framework.
    /// </summary>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection"/> to register the services
    /// </param>
    /// <param name="dbConfiguration">
    ///     The centralized <see cref="DatabaseConfiguration"/> applied to the entire application
    /// </param>
    /// <param name="configuration">
    ///     The <see cref="IConfiguration"/> to obtain configuration settings
    /// </param>
    /// <returns>
    ///     The <see cref="EfDataServiceConfigurator"/> for chaining operations
    /// </returns>
    public static EfDataServiceConfigurator AddEFCentralizedDataServices(
        this IServiceCollection serviceCollection,
        DatabaseConfiguration   dbConfiguration,
        IConfiguration          configuration)
    {
        serviceCollection.AddSingleton(dbConfiguration);

        serviceCollection.AddScoped<IRepository, Repository>();

        serviceCollection.AddSystemDateTimeProvider();

        serviceCollection.TryAddScoped<ITransactionWrapper, TransactionWrapper>();
        serviceCollection.TryAddScoped<ICurrentLoggedInUserResolver, DefaultCurrentUserResolver>();

        serviceCollection.Configure<DataMigrationOptions>(configuration.GetSection(nameof(DataMigrationOptions)));

        LinqToDBForEFTools.Initialize();

        var configurator = new EfDataServiceConfigurator
        {
            ServiceCollection = serviceCollection,
            DbConfiguration   = dbConfiguration
        };

        serviceCollection.TryAddSingleton(configurator);

        return configurator;
    }

    /// <summary>
    ///     Register the <typeparamref name="TDbContext"/> as the centralized <see cref="DbContext"/> for the application.
    /// </summary>
    /// <typeparam name="TDbContext">
    ///     The type of the <see cref="DbContext"/>
    /// </typeparam>
    /// <param name="configurator">
    ///     The <see cref="EfDataServiceConfigurator"/>
    /// </param>
    /// <param name="configureAction">
    ///     The action to configure <see cref="DbContextOptionsBuilder"/>
    /// </param>
    /// <returns>
    ///     The <see cref="EfDataServiceConfigurator"/> for chaining operations
    /// </returns>
    public static EfDataServiceConfigurator WithCentralizeDbContext<TDbContext>(
        this EfDataServiceConfigurator  configurator,
        Action<DbContextOptionsBuilder> configureAction)
        where TDbContext : DbContext
    {
        configurator.SharedDbContextOptionBuilder = configureAction;

        configurator.ServiceCollection.AddDbContext<TDbContext>(configure =>
        {
            configureAction?.Invoke(configure);

            if (configurator.DbConfiguration.UseLazyLoading)
                configure.UseLazyLoadingProxies();
        });

        configurator.ServiceCollection.AddScoped<DbContext, TDbContext>();

        return configurator;
    }

    /// <summary>
    ///     Add the migration <typeparamref name="TMigrationDbContext"/> to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    ///     <typeparamref name="TMigrationDbContext"/> is derived from <typeparamref name="IMigrationDefinitionDbContext"/>
    ///     and used for migrations
    /// </remarks>
    /// <typeparam name="TMigrationDbContext">
    ///     The <see cref="DbContext" /> used to apply migrations for the main application.
    /// </typeparam>
    /// <param name="configurator">
    ///     The <see cref="EfDataServiceConfigurator" /> to add the migration <typeparamref name="TMigrationDbContext"/>
    /// </param>
    public static EfDataServiceConfigurator UseCentralizedMigrationDbContext<TMigrationDbContext>(
        this EfDataServiceConfigurator  configurator)
        where TMigrationDbContext : DbContext, IMigrationDefinitionDbContext
    {
        var registeredDbContextType = configurator.ServiceCollection
                                                  .FirstOrDefault(d => d.ServiceType == typeof(DbContext))
                                                 ?.ImplementationType;

        if (registeredDbContextType is null)
        {
            throw new InvalidOperationException($"The centralized DbContext is not registered. " +
                                                $"Please call {nameof(WithCentralizeDbContext)}() to provide the centralized DbContext.");
        }


        var expectingBaseMigrationType = typeof(IMigrationDefinitionDbContext<>).MakeGenericType(registeredDbContextType);

        if (!typeof(TMigrationDbContext).IsAssignableTo(expectingBaseMigrationType))
            throw new InvalidOperationException($"The migration DbContext type {typeof(TMigrationDbContext).Name} must be use to apply migrations " +
                                                $"for the registered DbContext {registeredDbContextType.Name}.");

        configurator.ServiceCollection.AddDbContext<TMigrationDbContext>(configurator.SharedDbContextOptionBuilder);

        configurator.ServiceCollection.AddHostedService<AutoMigrateDbStartupTask<TMigrationDbContext>>();

        return configurator;
    }

    /// <summary>
    ///     Add the migration <typeparamref name="TMigrationDbContext"/> to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    ///     <typeparamref name="TMigrationDbContext"/> is derived from <typeparamref name="IMigrationDefinitionDbContext"/>
    ///     and used for migrations
    /// </remarks>
    /// <typeparam name="TMigrationDbContext">
    ///     The <see cref="DbContext" /> used for migrations
    /// </typeparam>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection" /> to add the migration <typeparamref name="TMigrationDbContext"/>
    /// </param>
    public static IServiceCollection UseMigrationDbContext<TMigrationDbContext>(
        this IServiceCollection         serviceCollection,
        Action<DbContextOptionsBuilder> configure)
        where TMigrationDbContext : DbContext, IMigrationDefinitionDbContext
    {
        serviceCollection.AddDbContext<TMigrationDbContext>(configure);

        serviceCollection.AddHostedService<AutoMigrateDbStartupTask<TMigrationDbContext>>();

        return serviceCollection;
    }

    /// <summary>
    ///     Adds the <typeparamref name="TBaseDbContext"/> and its migration <typeparamref name="TMigrationDbContext"/> to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    ///     <typeparamref name="TMigrationDbContext"/> is derived from <typeparamref name="IMigrationDefinitionDbContext"/>
    ///     and used for migrations
    /// </remarks>
    /// <typeparam name="TBaseDbContext">
    ///     The main <see cref="DbContext" />
    /// </typeparam>
    /// <typeparam name="TMigrationDbContext">
    ///     The <see cref="DbContext" /> used for migrations
    /// </typeparam>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection" /> to add the migration <typeparamref name="TMigrationDbContext"/>
    /// </param>
    public static IServiceCollection UseDbContextWithMigration<TBaseDbContext, TMigrationDbContext>(
        this IServiceCollection         serviceCollection,
        Action<DbContextOptionsBuilder> configure)
        where TMigrationDbContext : DbContext, IMigrationDefinitionDbContext<TBaseDbContext>
        where TBaseDbContext : DbContext
    {
        serviceCollection.AddDbContext<TBaseDbContext>(configure);
        serviceCollection.AddDbContext<TMigrationDbContext>(configure);

        serviceCollection.AddHostedService<AutoMigrateDbStartupTask<TMigrationDbContext>>();

        return serviceCollection;
    }
}