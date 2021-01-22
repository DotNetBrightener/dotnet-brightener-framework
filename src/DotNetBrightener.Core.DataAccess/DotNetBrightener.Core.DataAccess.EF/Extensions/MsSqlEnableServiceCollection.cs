using DotNetBrightener.Core.DataAccess.Abstractions;
using DotNetBrightener.Core.DataAccess.EF.Migrations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetBrightener.Core.DataAccess.EF.Extensions
{
    public static class MsSqlEnableServiceCollection
    {
        public static IServiceCollection UseSqlServerDbContextsRegistration(this IServiceCollection serviceCollection)
        {
            serviceCollection.RegisterDbProviderConfigure(DatabaseProvider.MsSql, ConfigureMsSqlDbContexts);

            return serviceCollection;
        }


        private static void ConfigureMsSqlDbContexts(DbContextOptionsBuilder dbContextOptions,
                                                     DatabaseConfiguration   databaseConfiguration)
        {
            dbContextOptions.UseSqlServer(databaseConfiguration.ConnectionString);
        }
    }

    public static class EnableDataAccessServiceCollectionExtensions
    {
        public static IServiceCollection EnableEntityFramework(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddScoped<ISchemaMigrationExecutor, SchemaMigrationExecutor>();

            return serviceCollection;
        }
    }
}