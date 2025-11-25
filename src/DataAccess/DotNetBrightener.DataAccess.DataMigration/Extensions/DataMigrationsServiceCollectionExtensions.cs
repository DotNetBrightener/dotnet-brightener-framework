using DotNetBrightener.DataAccess.DataMigration;
using DotNetBrightener.DataAccess.DataMigration.Extensions;
using System.Reflection;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DataMigrationsServiceCollectionExtensions
{
    /// <summary>
    ///     Detects the data migrations and registers them to <see cref="IServiceCollection" />
    /// </summary>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection" /> 
    /// </param>
    /// <returns>
    ///     The same instance of <param name="serviceCollection" /> for chaining operations
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static DataMigrationConfiguration EnableDataMigrations(this IServiceCollection serviceCollection)
    {
        var configuration = new DataMigrationConfiguration
        {
            ServiceCollection = serviceCollection
        };

        serviceCollection.AddHostedService<DataMigrationRunner>();

        var metadata = new DataMigrationMetadata();

        serviceCollection.AddSingleton(metadata);
        serviceCollection.AddSingleton(configuration);

        return configuration;
    }

    /// <summary>
    ///     Scans the whole execution context of the current application to detect the data migration classes
    /// </summary>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection" />
    /// </param>
    /// <returns>
    ///     The same instance of <param name="serviceCollection" /> for chaining operations
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AutoScanDataMigrators(this IServiceCollection serviceCollection)
    {
        var assemblies = AppDomain.CurrentDomain
                                  .GetAssemblies()
                                  .FilterSkippedAssemblies()
                                  .ToArray();

        var types = assemblies.FilterSkippedAssemblies()
                              .GetDerivedTypes<IDataMigration>()
                              .ToList();

        if (types.Count == 0)
        {
            return serviceCollection;
        }

        var metadata = RetrieveMigrationMetadata(serviceCollection);

        foreach (var type in types)
        {
            AddDataMigratorType(serviceCollection, type, metadata);
        }

        return serviceCollection;
    }

    /// <summary>
    ///     Scans the assembly that calls this method to detect the data migration classes
    /// </summary>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection" />
    /// </param>
    /// <returns>
    ///     The same instance of <param name="serviceCollection" /> for chaining operations
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection ScanDataMigratorsFromCurrentAssembly(this IServiceCollection serviceCollection)
    {
        var assemblies = new[]
        {
            Assembly.GetCallingAssembly()
        };

        var types = assemblies.FilterSkippedAssemblies()
                              .GetDerivedTypes<IDataMigration>()
                              .ToList();

        if (types.Count == 0)
        {
            return serviceCollection;
        }

        var metadata = RetrieveMigrationMetadata(serviceCollection);

        foreach (var type in types)
        {
            AddDataMigratorType(serviceCollection, type, metadata);
        }

        return serviceCollection;
    }

    /// <summary>
    ///     Adds the specified data migration class to the service collection
    /// </summary>
    /// <typeparam name="TMigration">
    ///     The type of data migration
    /// </typeparam>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection" />
    /// </param>
    /// <returns>
    ///     The same instance of <param name="serviceCollection" /> for chaining operations
    /// </returns>
    public static IServiceCollection AddDataMigrator<TMigration>(this IServiceCollection serviceCollection)
        where TMigration : IDataMigration
    {
        var type = typeof(TMigration);

        return AddDataMigratorType(serviceCollection, type);
    }

    /// <summary>
    ///    Adds the specified data migration class to the service collection
    /// </summary>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection" />
    /// </param>
    /// <param name="type">
    ///     The type of data migration
    /// </param>
    /// <returns>
    ///     The same instance of <param name="serviceCollection" /> for chaining operations
    /// </returns>
    public static IServiceCollection AddDataMigratorType(IServiceCollection serviceCollection,
                                                         Type               type)
    {
        var metadata = RetrieveMigrationMetadata(serviceCollection);

        return AddDataMigratorType(serviceCollection, type, metadata);
    }

    private static IServiceCollection AddDataMigratorType(IServiceCollection serviceCollection,
                                                         Type               type, 
                                                         DataMigrationMetadata metadata)
    {
        if (!type.IsAssignableTo(typeof(IDataMigration)))
        {
            throw new
                InvalidOperationException($"The class {type.FullName} must implement `{nameof(IDataMigration)}` in order to act as a data migrator class");
        }

        if (type.GetCustomAttribute<DataMigrationAttribute>() is not { } attribute)
        {
            throw new
                InvalidOperationException($"The data migration {type.FullName} must have [DataMigration] attribute defined with the migration id");
        }
        
        metadata.Add(attribute.MigrationId, type);
        serviceCollection.AddScoped(typeof(IDataMigration), type);
        serviceCollection.AddScoped(type);

        return serviceCollection;
    }

    private static DataMigrationMetadata RetrieveMigrationMetadata(IServiceCollection serviceCollection)
    {
        var metadataServiceDescriptor =
            serviceCollection.FirstOrDefault(_ => _.ServiceType == typeof(DataMigrationMetadata));

        var metadata = metadataServiceDescriptor?.ImplementationInstance as DataMigrationMetadata;

        if (metadata is null)
        {
            throw new
                InvalidOperationException($"The data migrations must be enabled first using {nameof(EnableDataMigrations)} method");
        }

        return metadata;
    }
}