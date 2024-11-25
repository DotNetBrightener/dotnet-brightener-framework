using DotNetBrightener.DataAccess.EF.Migrations;
using DotNetBrightener.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.Auditing.Tests.DbContexts;

public class TestEntity : BaseEntityWithAuditInfo
{
    public string Name { get; set; }

    public bool BooleanValue { get; set; }

    public DateTimeOffset? DateTimeOffsetValue { get; set; }

    public int IntValue { get; set; }

    public int AnotherIntValue { get; set; }
}

public class TestAuditingDbContext(DbContextOptions<TestAuditingDbContext> options) : AdvancedDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestEntity>(e =>
        {
            e.Property(x => x.Name)
             .HasMaxLength(4000);
        });
    }
}