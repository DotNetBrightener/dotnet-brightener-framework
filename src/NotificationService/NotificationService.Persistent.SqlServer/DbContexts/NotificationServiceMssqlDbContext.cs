using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NotificationService.DbContexts;

namespace NotificationService.Persistent.SqlServer.DbContexts;

internal class SqlServerDbContextDesignTimeFactory : SqlServerDbContextDesignTimeFactory<NotificationServiceMssqlDbContext>;

internal class NotificationServiceMssqlDbContext(DbContextOptions<NotificationServiceMssqlDbContext> options)
    : NotificationServiceBaseDbContext(options);