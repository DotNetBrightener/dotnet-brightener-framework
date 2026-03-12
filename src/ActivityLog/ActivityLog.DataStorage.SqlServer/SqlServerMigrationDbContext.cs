using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;

namespace ActivityLog.DataStorage.SqlServer;

internal class SqlServerMigrationDbContext(DbContextOptions<SqlServerMigrationDbContext> options)
    : ActivityLogDbContext(options), IMigrationDefinitionDbContext<ActivityLogDbContext>;
