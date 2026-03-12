using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NotificationService.DbContexts;

namespace NotificationService.Persistent.PostgreSQL.DbContexts;

internal class PostgreSqlDbContextDesignTimeFactory : PostgreSqlDbContextDesignTimeFactory<NotificationServiceNpgsqlDbContext>;

internal class NotificationServiceNpgsqlDbContext(DbContextOptions<NotificationServiceNpgsqlDbContext> options)
    : NotificationServiceBaseDbContext(options);