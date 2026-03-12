using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ActivityLog.DataStorage.PostgreSql;

internal class DesignTimeAppDbContext : PostgreSqlDbContextDesignTimeFactory<PostgreSqlMigrationDbContext>;