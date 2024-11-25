using DotNetBrightener.DataAccess.EF.Migrations;
using DotNetBrightener.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotNetBrightener.DataAccess.Auditing.WebTest.DbContexts;

public class TestEntity : BaseEntityWithAuditInfo
{
    public string EntityName { get; set; }
}

internal class SqlMigrationDbFactory : SqlServerDbContextDesignTimeFactory<MainAppDbContext>;

public class MainAppDbContext : AdvancedDbContext, IMigrationDefinitionDbContext<MainAppDbContext>
{
    public MainAppDbContext(DbContextOptions<MainAppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestEntity>();
    }
}