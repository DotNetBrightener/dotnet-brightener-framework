#nullable enable
using DotNetBrightener.DataAccess.EF.PostgreSQL.History;
using DotNetBrightener.DataAccess.EF.PostgreSQL.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL.Extensions;

/// <summary>
/// Extension methods for configuring PostgreSQL history tables
/// </summary>
public static class PostgreSqlHistoryModelBuilderExtensions
{
    /// <summary>
    /// Adds PostgreSQL history interceptor to the DbContext options
    /// </summary>
    /// <param name="optionsBuilder">The options builder</param>
    /// <param name="logger">Optional logger</param>
    /// <returns>The options builder for chaining</returns>
    public static DbContextOptionsBuilder AddPostgreSqlHistoryInterceptor(this DbContextOptionsBuilder optionsBuilder,
                                                                          IServiceProvider             serviceProvider)
    {
        var historyTableManager = new PostgreSqlHistoryTableManager(serviceProvider);

        var logger = serviceProvider.GetService<ILogger<PostgreSqlHistoryInterceptor>>();

        var interceptor = new PostgreSqlHistoryInterceptor(logger!, historyTableManager);

        optionsBuilder.AddInterceptors(interceptor);

        return optionsBuilder;
    }
}