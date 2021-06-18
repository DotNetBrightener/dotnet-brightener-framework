using DotNetBrightener.Core.DataAccess.Abstractions;
using DotNetBrightener.Core.DataAccess.Abstractions.Repositories;
using DotNetBrightener.Core.DataAccess.Abstractions.Transaction;
using DotNetBrightener.Core.DataAccess.Providers;
using DotNetBrightener.Core.DataAccess.Repositories;
using DotNetBrightener.Core.DataAccess.Transaction;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DotNetBrightener.Core.DataAccess.Extensions
{

    public static class EnableDataAccessServiceCollectionExtensions
    {
        public static IServiceCollection EnableDataAccess(this IServiceCollection serviceCollection,
            Action<DataAccessConfiguration> configure = null)
        {
            serviceCollection.AddSingleton<IDotNetBrightenerDataProvider, MsSqlDataProvider>();

            serviceCollection.AddScoped<ITransactionManager, TransactionManager>();
            serviceCollection.AddScoped<IDataProviderFactory, DataProviderFactory>();
            serviceCollection.AddScoped<IBaseRepository, BaseRepository>();
            serviceCollection.AddScoped<IDataWorkContext, DataWorkContext>();

            var configuration = new DataAccessConfiguration();

            configure?.Invoke(configuration);
            serviceCollection.AddSingleton(configuration);

            return serviceCollection;
        }
    }
}