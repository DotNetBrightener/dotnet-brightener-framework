using CRUDWebApiWithGeneratorDemo.Core.Entities;
using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CRUDWebApiWithGeneratorDemo.Database.DbContexts;

public class DesignTimeAppDbContext : SqlServerDbContextDesignTimeFactory<MainAppDbContext>;

public class MainAppDbContext(DbContextOptions<MainAppDbContext> options)
    : SqlServerVersioningMigrationEnabledDbContext(options)
{
    protected override void ConfigureModelBuilder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>();

        modelBuilder.Entity<ProductCategory>();

        modelBuilder.Entity<ProductDocument>(document =>
        {
            document.Property(_ => _.Price)
                    .HasColumnType("decimal");
        });

        modelBuilder.Entity<GroupEntity>();
    }
}