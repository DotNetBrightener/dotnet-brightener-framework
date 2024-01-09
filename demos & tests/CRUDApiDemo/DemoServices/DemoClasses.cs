using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using DotNetBrightener.Core.Logging;

namespace CRUDApiDemo.DemoServices;

public class TestEntity
{
    [Key]
    public long Id { get; set; }


    [MaxLength(512)]
    public string Value { get; set; }
}

public class DemoDbContext : DbContext
{
    public DemoDbContext(DbContextOptions<DemoDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventLog>();
        modelBuilder.Entity<TestEntity>();
    }
}