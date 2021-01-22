using DotNetBrightener.Core.DataAccess.Abstractions;
using DotNetBrightener.Core.DataAccess.EF.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Core.DataAccess.EF.PostgreSQL
{
    public static class PostgreSqlEnableServiceCollection
    {
        /// <summary>
        ///     Enables the ability of registering db context using PostgreSQL database provider
        /// </summary>
        /// <param name="serviceCollection"></param>
        public static void UsePostgreSqlDbContextRegistration(this IServiceCollection serviceCollection)
        {
            serviceCollection.RegisterDbProviderConfigure(DatabaseProvider.PostgreSql, ConfigurePostgreSql);
        }

        private static void ConfigurePostgreSql(DbContextOptionsBuilder dbContextOptions, DatabaseConfiguration databaseConfiguration)
        {
            dbContextOptions.UseNpgsql(databaseConfiguration.ConnectionString);
        }
    }
}