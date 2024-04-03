using DotNetBrightener.Core.Logging;
using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebAppCommonShared.Demo.DbContexts;

public class DesignTimeAppDbContext : SqlServerDbContextDesignTimeFactory<MainAppDbContext>
{
}

public class MainAppDbContext : SqlServerVersioningMigrationEnabledDbContext
{
    public MainAppDbContext(DbContextOptions<MainAppDbContext> options)
        : base(options)
    {

    }

    protected override void ConfigureModelBuilder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventLog>();
    }
}