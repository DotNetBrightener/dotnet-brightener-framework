using DotNetBrightener.DataAccess.EF.Extensions;
using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using WebAppCommonShared.Demo.Entities;

namespace WebAppCommonShared.Demo.DbContexts;

public class DesignTimeAppDbContext : SqlServerDbContextDesignTimeFactory<MainDbContext>{}


public class MainDbContext: SqlServerVersioningMigrationEnabledDbContext, IMigrationDefinitionDbContext<MainDbContext>
{
    public MainDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void ConfigureModelBuilder(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        modelBuilder.RegisterEntity<Subscription>();

        RegisterEnumLookupTable<SubscriptionStatus>(modelBuilder);
    }
}