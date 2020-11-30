using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Core.DataAccess.EF.Extensions
{
    public static class DbProviderRegistrationExtensions
    {
        public static void RegisterDbProviderConfigure(this IServiceCollection                                serviceCollection,
                                                       DatabaseProvider                                       databaseProvider,
                                                       Action<DbContextOptionsBuilder, DatabaseConfiguration> configure)
        {
            DbContextOptionConfigure.Instance.RegisterDbContextOptionConfigure(databaseProvider, configure);
        }
    }
}