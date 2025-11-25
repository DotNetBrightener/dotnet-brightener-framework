using DotNetBrightener.DataAccess.Auditing.Storage.DbContexts;
using DotNetBrightener.DataAccess.Auditing.Storage.EventHandlers;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.EntityFrameworkCore;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class AuditingStorageServiceCollectionExtensions
{
    public static IServiceCollection AddAuditingSqlServerStorage(this IServiceCollection serviceCollection,
                                                                 string                  connectionString)
    {
        serviceCollection.AddDbContext<MssqlStorageAuditingDbContext>((optionBuilder) =>
        {
            optionBuilder.UseSqlServer(connectionString,
                                       contextOptionsBuilder =>
                                       {
                                           contextOptionsBuilder
                                              .MigrationsHistoryTable("__MigrationsHistory",
                                                                      MssqlStorageAuditingDbContext.SchemaName);

                                           contextOptionsBuilder.EnableRetryOnFailure(20);
                                       });
        });

        serviceCollection.AddScoped<IEventHandler, SaveAuditTrailService>();

        return serviceCollection;
    }
}