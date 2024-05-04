using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.Infrastructure.AppClientManager.DataStorage.PostgreSql;

internal class MigrationPostgreSqlDbContext(DbContextOptions<MigrationPostgreSqlDbContext> options)
    : AppClientDbContext(options), IMigrationDefinitionDbContext<AppClientDbContext>;