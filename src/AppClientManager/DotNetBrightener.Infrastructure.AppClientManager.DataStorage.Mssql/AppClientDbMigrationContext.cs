using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotNetBrightener.Infrastructure.AppClientManager.DataStorage.Mssql;


public class DesignTimeAppDbContext : SqlServerDbContextDesignTimeFactory<AppClientDbMigrationContext>
{
}

public class AppClientDbMigrationContext : AppClientDbContext, IMigrationDefinitionDbContext<AppClientDbContext>
{
    public AppClientDbMigrationContext(DbContextOptions<AppClientDbMigrationContext> options)
        : base(options)
    {

    }

    public AppClientDbMigrationContext(DbContextOptions<AppClientDbMigrationContext> options,
                                       DatabaseConfiguration                         databaseConfiguration)
        : base(options, optionBuilder => optionBuilder.UseSqlServer(databaseConfiguration.ConnectionString))
    {
    }
}