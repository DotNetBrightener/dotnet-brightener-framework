using System;
using DotNetBrightener.Core.DataAccess.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Core.DataAccess.EF.Extensions
{
    public static class RegisterDbContextServiceCollectionExtensions
    {

        /// <summary>
        ///     Registers the <typeparamref name="TDbContextToRegister"/> to service collection,
        ///     optionally specifies whether we want to register it as <typeparamref name="TBaseDbContext"/>.
        /// </summary>
        /// <typeparam name="TDbContextToRegister">The type of <see cref="DbContext"/> to register</typeparam>
        /// <typeparam name="TBaseDbContext">The type of <see cref="DbContext"/> to register as, if <see cref="registerAsDbContext"/> is <c>true</c></typeparam>
        /// <param name="serviceCollection"></param>
        /// <param name="databaseConfiguration">The database configuration</param>
        /// <param name="registerAsDbContext">
        ///     Specifies if a registration as <see cref="TBaseDbContext"/> is needed.
        ///     If <c>true</c>, the resolve of <see cref="TBaseDbContext"/> will also return the registered type from the <see cref="IServiceProvider"/>.
        /// </param>
        public static void RegisterDbContext<TDbContextToRegister, TBaseDbContext>(
            this IServiceCollection serviceCollection,
            DatabaseConfiguration   databaseConfiguration,
            bool                    registerAsDbContext = true)
            where TDbContextToRegister : DbContext
            where TBaseDbContext : DbContext
        {
            RegisterDbContext<TBaseDbContext>(serviceCollection,
                                              typeof(TDbContextToRegister),
                                              databaseConfiguration,
                                              registerAsDbContext);
        }

        /// <summary>
        ///     Registers the <see cref="dbContextType"/> to service collection, optionally specifies whether we want to register it as <see cref="DbContext"/>.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="dbContextType">The derived type of the DbContext to register</param>
        /// <param name="databaseConfiguration">The database configuration</param>
        /// <param name="registerAsDbContext">
        ///     Specifies if a registration as <see cref="DbContext"/> is needed.
        ///     If <c>true</c>, the resolve of <see cref="DbContext"/> will also return the registered type from the <see cref="IServiceProvider"/>.
        /// </param>
        public static void RegisterDbContext(this IServiceCollection serviceCollection,
                                             Type                    dbContextType,
                                             DatabaseConfiguration   databaseConfiguration,
                                             bool                    registerAsDbContext = true)
        {
            RegisterDbContext<DbContext>(serviceCollection,
                                         dbContextType,
                                         databaseConfiguration,
                                         registerAsDbContext);
        }

        /// <summary>
        ///     Registers the <see cref="dbContextType"/> to service collection, optionally specifies whether we want to register it as <see cref="DbContext"/>.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="dbContextType">The derived type of the DbContext to register</param>
        /// <param name="databaseConfiguration">The database configuration</param>
        /// <param name="registerAsDbContext">
        ///     Specifies if a registration as <see cref="TBaseDbContext"/> is needed.
        ///     If <c>true</c>, the resolve of <see cref="TBaseDbContext"/> will also return the registered type from the <see cref="IServiceProvider"/>.
        /// </param>
        public static void RegisterDbContext<TBaseDbContext>(this IServiceCollection serviceCollection,
                                                             Type                    dbContextType,
                                                             DatabaseConfiguration   databaseConfiguration,
                                                             bool                    registerAsDbContext = true)
            where TBaseDbContext : DbContext
        {
            var dbContextOptionsBuilderType = typeof(DbContextOptionsBuilder<>);

            var genericType = dbContextOptionsBuilderType.MakeGenericType(dbContextType);

            if (Activator.CreateInstance(genericType) is DbContextOptionsBuilder dbContextOptions)
            {
                DbContextOptionConfigure.Instance
                                        .ConfigureDbProvider(dbContextOptions, databaseConfiguration);

                if (databaseConfiguration.UseLazyLoadingProxies)
                    dbContextOptions.UseLazyLoadingProxies();

                serviceCollection.AddSingleton(typeof(DbContextOptions<>).MakeGenericType(dbContextType),
                                               dbContextOptions.Options);
            }

            serviceCollection.AddScoped(dbContextType, dbContextType);

            if (registerAsDbContext)
            {
                serviceCollection.Add(ServiceDescriptor.Scoped(typeof(TBaseDbContext), dbContextType));
            }
        }
    }
}