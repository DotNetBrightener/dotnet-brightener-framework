using DotNetBrightener.DataAccess.Dapper.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetBrightener.DataAccess.Dapper;

public static class MsSqlDapperServiceCollectionExtensions
{
    public static void AddMsSqlDapperDataAccessLayer(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IDapperRepository, MsSqlDapperRepository>();
        serviceCollection.TryAddScoped<ICurrentLoggedInUserResolver, DefaultCurrentUserResolver>();
    }
}