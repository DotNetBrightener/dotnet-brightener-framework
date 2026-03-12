using DotNetBrightener.DataAccess.EF.Repositories;
using Microsoft.Extensions.Logging;
using NotificationService.Persistent.SqlServer.DbContexts;
using NotificationService.Repository;

namespace NotificationService.Persistent.SqlServer;

internal class NotificationServiceSqlServerRepository(
    NotificationServiceMssqlDbContext dbContext,
    IServiceProvider                  serviceProvider,
    ILoggerFactory                    loggerFactory)
    : Repository<NotificationServiceMssqlDbContext>(dbContext, serviceProvider, loggerFactory),
      INotificationMessageQueueRepository;