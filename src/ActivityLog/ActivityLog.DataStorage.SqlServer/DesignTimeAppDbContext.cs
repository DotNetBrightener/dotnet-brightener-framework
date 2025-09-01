using Microsoft.EntityFrameworkCore.Design;

namespace ActivityLog.DataStorage.SqlServer;

internal class DesignTimeAppDbContext : SqlServerDbContextDesignTimeFactory<SqlServerMigrationDbContext>;