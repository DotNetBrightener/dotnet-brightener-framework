using ActivityLog.DataStorage;
using Microsoft.EntityFrameworkCore;

namespace ActivityLog.DataStorage.PostgreSql;

public class PostgreSqlMigrationDbContext : ActivityLogDbContext
{
    public PostgreSqlMigrationDbContext(DbContextOptions<PostgreSqlMigrationDbContext> options)
        : base(options)
    {
    }
}
