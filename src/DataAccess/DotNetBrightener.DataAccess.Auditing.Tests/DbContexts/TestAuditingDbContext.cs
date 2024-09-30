using System.ComponentModel.DataAnnotations;
using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.Auditing.Tests.DbContexts;

public class TestEntity
{
    [Key]
    public long Id { get; set; }

    public string Name { get; set; }

    public bool BooleanValue { get; set; }

    public DateTimeOffset? DateTimeOffsetValue { get; set; }

    public int IntValue { get; set; }
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