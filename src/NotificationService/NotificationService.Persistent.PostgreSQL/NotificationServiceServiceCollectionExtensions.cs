using Microsoft.EntityFrameworkCore;
using NotificationService.Extensions;
using NotificationService.Persistent.PostgreSQL;
using NotificationService.Persistent.PostgreSQL.DbContexts;
using NotificationService.Repository;
using System.Data.Common;
using Npgsql;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class NotificationServiceServiceCollectionExtensions
{
    extension(NotificationServiceBuilder notificationServiceBuilder)
    {
        public void UseNpgsql(Func<IServiceProvider, DbContextOptionsBuilder, DbConnection> builder)
        {
            var serviceCollection = notificationServiceBuilder.Services;

            serviceCollection.AddDbContext<NotificationServiceNpgsqlDbContext>((serviceProvider,
                                                                                optionsBuilder) =>
            {
                var connection = builder(serviceProvider, optionsBuilder);

                optionsBuilder.UseNpgsql(connection,
                                         contextOptionsBuilder =>
                                         {
                                             contextOptionsBuilder
                                                .MigrationsHistoryTable("__MigrationsHistory",
                                                                        NotificationServiceNpgsqlDbContext.SchemaName);
                                         })
                              .UseLazyLoadingProxies();
            });

            serviceCollection
               .Where(x => x.ServiceType == typeof(INotificationMessageQueueRepository))
               .ToList()
               .ForEach(x => serviceCollection.Remove(x));

            serviceCollection.AddScoped<INotificationMessageQueueRepository, NotificationServicePostgreSqlRepository>();
            serviceCollection.AddAutoMigrationForDbContextAfterAppStarted<NotificationServiceNpgsqlDbContext>();
        }

        public void UseNpgsql(string connectionString)
        {
            notificationServiceBuilder.UseNpgsql((serviceProvider, builder) => new NpgsqlConnection(connectionString));
        }
    }
}