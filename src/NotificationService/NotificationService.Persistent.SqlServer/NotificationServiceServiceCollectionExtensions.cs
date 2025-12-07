using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NotificationService.Extensions;
using NotificationService.Persistent.SqlServer.DbContexts;
using NotificationService.Repository;
using System.Data.Common;
using NotificationService.Persistent.SqlServer;


// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class NotificationServiceServiceCollectionExtensions
{
    extension(NotificationServiceBuilder notificationServiceBuilder)
    {
        public void UseSqlServer(Func<IServiceProvider, DbContextOptionsBuilder, DbConnection> builder)
        {
            var serviceCollection = notificationServiceBuilder.Services;
            serviceCollection.AddDbContext<NotificationServiceMssqlDbContext>((serviceProvider,
                                                                               optionsBuilder) =>
            {
                var connection = builder(serviceProvider, optionsBuilder);

                optionsBuilder.UseSqlServer(connection,
                                            contextOptionsBuilder =>
                                            {
                                                contextOptionsBuilder
                                                   .MigrationsHistoryTable("__MigrationsHistory",
                                                                           NotificationServiceMssqlDbContext
                                                                              .SchemaName);
                                            })
                              .UseLazyLoadingProxies();
            });

            serviceCollection
               .Where(x => x.ServiceType == typeof(INotificationMessageQueueRepository))
               .ToList()
               .ForEach(x => serviceCollection.Remove(x));

            serviceCollection.AddScoped<INotificationMessageQueueRepository, NotificationServiceSqlServerRepository>();
            serviceCollection.AddAutoMigrationForDbContextAfterAppStarted<NotificationServiceMssqlDbContext>();
        }

        public void UseSqlServer(string connectionString)
        {
            notificationServiceBuilder.UseSqlServer((serviceProvider, builder) => new SqlConnection(connectionString));
        }
    }
}