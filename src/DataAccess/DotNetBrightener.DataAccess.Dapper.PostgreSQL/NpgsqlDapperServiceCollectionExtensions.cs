using DotNetBrightener.DataAccess.Dapper.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.DataAccess.Dapper.PostgreSQL;

public static class NpgsqlDapperServiceCollectionExtensions
{
    public static void AddNpgsqlDapperDataAccessLayer(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IDapperRepository, NpgsqlDapperRepository>();
    }
}