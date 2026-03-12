#nullable enable
using DotNetBrightener.DataAccess.EF.Migrations;
using DotNetBrightener.DataAccess.EF.PostgreSQL.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL.Extensions;

internal class PostgreSQlHistoryEnabledDbContextConfigurator(
    PostgreSqlHistoryInterceptor postgreSqlHistoryInterceptor) : IDbContextConfigurator
{
    public void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(postgreSqlHistoryInterceptor);
    }
}