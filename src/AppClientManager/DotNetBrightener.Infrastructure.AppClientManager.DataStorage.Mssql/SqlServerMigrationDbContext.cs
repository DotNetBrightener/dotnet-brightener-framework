using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.Infrastructure.AppClientManager.DataStorage.Mssql;

internal class SqlServerMigrationDbContext(
    DbContextOptions<SqlServerMigrationDbContext> options)
    : AppClientDbContext(options), IMigrationDefinitionDbContext<AppClientDbContext>;