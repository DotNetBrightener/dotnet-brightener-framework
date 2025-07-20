using DotNetBrightener.DataAccess.EF.PostgreSQL;
using DotNetBrightener.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess_PostgreMigrations_Test.Db.DbContexts;

public class MainDbContext : PostgreSqlVersioningMigrationEnabledDbContext
{
    protected MainDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public MainDbContext(DbContextOptions<MainDbContext> options)
        : base(options)
    {
    }

    protected override void ConfigureModelBuilder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>();
    }
}

public class TestEntity : GuidBaseEntity;