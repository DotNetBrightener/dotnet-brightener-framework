using DotNetBrightener.Core.DataAccess.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Core.DataAccess.PostgreSQL
{
    public static class PostgreSqlEnableServiceCollection
    {
        public static IServiceCollection AddPostgreSqlDataAccess(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IDotNetBrightenerDataProvider, PostgreSqlDataProvider>();

            return serviceCollection;
        }
    }
}