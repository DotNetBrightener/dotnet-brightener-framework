using CRUDWebApiWithGeneratorDemo.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CRUDWebApiWithGeneratorDemo.Database.DbContexts;

public class DesignTimeAppDbContext : SqlServerDbContextDesignTimeFactory<MainAppDbContext>
{
}

public class MainAppDbContext : DbContext
{
    public MainAppDbContext(DbContextOptions<MainAppDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>();
        modelBuilder.Entity<ProductCategory>();
        modelBuilder.Entity<ProductDocument>();
    }
}