using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.EF.Tests;

public class TestDbContext: DbContext
{
    public readonly Guid Id = Guid.NewGuid();

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>();
    }
}