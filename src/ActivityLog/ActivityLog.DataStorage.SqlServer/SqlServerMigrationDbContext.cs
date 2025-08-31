using ActivityLog.DataStorage;
using Microsoft.EntityFrameworkCore;

namespace ActivityLog.DataStorage.SqlServer;

public class SqlServerMigrationDbContext : ActivityLogDbContext
{
    public SqlServerMigrationDbContext(DbContextOptions<SqlServerMigrationDbContext> options)
        : base(options)
    {
    }
}
