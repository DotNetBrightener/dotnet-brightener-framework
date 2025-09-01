using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;

namespace ActivityLog.DataStorage.PostgreSql;

internal class PostgreSqlMigrationDbContext(DbContextOptions<PostgreSqlMigrationDbContext> options)
    : ActivityLogDbContext(options), IMigrationDefinitionDbContext<ActivityLogDbContext>;