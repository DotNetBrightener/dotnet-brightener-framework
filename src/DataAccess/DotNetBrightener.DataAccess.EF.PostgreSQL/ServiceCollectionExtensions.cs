using DotNetBrightener.DataAccess.EF.PostgreSQL;
using Microsoft.EntityFrameworkCore;

// ReSharper disable CheckNamespace

namespace  Microsoft.Extensions.DependencyInjection.Extensions;


public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Enables the support for <see cref="Guid"/> v7 for PostgreSQL database contexts.
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    /// <param name="serviceCollection"></param>
    /// <returns></returns>
    public static IServiceCollection EnableGuidV7ForPostgreSql<TDbContext>(this IServiceCollection serviceCollection) where TDbContext: DbContext
    {
        serviceCollection.TryAddSingleton<UuidV7EnablerForPostgreSqlDbContexts<TDbContext>>();

        return serviceCollection;
    }
}