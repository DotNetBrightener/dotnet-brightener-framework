using DotNetBrightener.DataAccess.Dapper.Abstractions;
using DotNetBrightener.DataAccess.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetBrightener.DataAccess.Dapper.PostgreSQL;

public static class NpgsqlDapperServiceCollectionExtensions
{
    public static void AddNpgsqlDapperDataAccessLayer(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IDapperRepository, NpgsqlDapperRepository>();

        serviceCollection.TryAddScoped<ITransactionWrapper, TransactionWrapper>();
        serviceCollection.TryAddScoped<ICurrentLoggedInUserResolver, DefaultCurrentUserResolver>();
    }
}