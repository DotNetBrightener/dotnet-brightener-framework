#nullable enable
using DotNetBrightener.DataAccess.EF.Migrations;
using DotNetBrightener.DataAccess.EF.PostgreSQL.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL.Extensions;

/// <summary>
/// 	DbContext configurator that adds PostgreSQL history tracking interceptors
/// </summary>
internal class PostgreSQlHistoryEnabledDbContextConfigurator(
    PostgreSqlHistoryInterceptor postgreSqlHistoryInterceptor,
    PostgreSqlHistorySaveChangesInterceptor postgreSqlHistorySaveChangesInterceptor) : IDbContextConfigurator
{
    /// <summary>
    /// 	Configures the DbContext to use PostgreSQL history tracking interceptors
    /// </summary>
    /// <param name="optionsBuilder">The options builder to configure</param>
    public void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(postgreSqlHistoryInterceptor, 
                                       postgreSqlHistorySaveChangesInterceptor);
    }
}