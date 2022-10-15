using DotNetBrightener.Core.DataAccess.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
}