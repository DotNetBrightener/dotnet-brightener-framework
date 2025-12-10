using DotNetBrightener.DataAccess.Auditing.Storage.DbContexts;
using DotNetBrightener.DataAccess.Auditing.Storage.EventHandlers;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Data.Common;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class AuditingStorageServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddAuditingSqlServerStorage(string             connectionString)
        {
            return serviceCollection.AddAuditingSqlServerStorage((provider, builder) =>
                                                                     new SqlConnection(connectionString));
        }

        public IServiceCollection AddAuditingSqlServerStorage(Func<IServiceProvider, DbContextOptionsBuilder,
                                                                      DbConnection>
                                                                  connectionStringResolver)
        {
            serviceCollection.AddDbContext<MssqlStorageAuditingDbContext>((serviceProvider, options) =>
            {
                var connection = connectionStringResolver.Invoke(serviceProvider, options);

                options.UseSqlServer(connection,
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
}