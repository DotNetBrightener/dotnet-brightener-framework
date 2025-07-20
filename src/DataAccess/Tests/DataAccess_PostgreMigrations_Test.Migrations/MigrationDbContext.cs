using DataAccess_PostgreMigrations_Test.Db.DbContexts;
using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DataAccess_PostgreMigrations_Test.Migrations;

public class MigrationDbContext(DbContextOptions<MigrationDbContext> options) : MainDbContext(options), IMigrationDefinitionDbContext<MainDbContext>;

internal class DesignTimeAppDbContext : PostgreSqlDbContextDesignTimeFactory<MigrationDbContext>;