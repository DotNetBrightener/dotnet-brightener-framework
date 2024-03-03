using DotNetBrightener.DataAccess.Dapper.Abstractions;
using DotNetBrightener.DataAccess.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetBrightener.DataAccess.Dapper;

public static class MsSqlDapperServiceCollectionExtensions
{
    public static void AddMsSqlDapperDataAccessLayer(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IDapperRepository, MsSqlDapperRepository>();

        serviceCollection.TryAddScoped<ITransactionWrapper, TransactionWrapper>();
        serviceCollection.TryAddScoped<ICurrentLoggedInUserResolver, DefaultCurrentUserResolver>();
    }
}