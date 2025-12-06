using DotNetBrightener.DataAccess.EF.Repositories;
using Microsoft.Extensions.Logging;
using NotificationService.Persistent.PostgreSQL.DbContexts;
using NotificationService.Repository;

namespace NotificationService.Persistent.PostgreSQL;

internal class NotificationServicePostgreSqlRepository(
    NotificationServiceNpgsqlDbContext dbContext,
    IServiceProvider                   serviceProvider,
    ILoggerFactory                     loggerFactory)
    : Repository<NotificationServiceNpgsqlDbContext>(dbContext, serviceProvider, loggerFactory),
      INotificationMessageQueueRepository;