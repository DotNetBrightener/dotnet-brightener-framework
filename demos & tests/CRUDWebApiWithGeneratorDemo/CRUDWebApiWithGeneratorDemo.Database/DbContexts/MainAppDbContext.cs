using CRUDWebApiWithGeneratorDemo.Core.Entities;
using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CRUDWebApiWithGeneratorDemo.Database.DbContexts;

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
        var productEntity         = modelBuilder.Entity<Product>();
        var productCategoryEntity = modelBuilder.Entity<ProductCategory>();
        var document              = modelBuilder.Entity<ProductDocument>();

        document.Property(_ => _.Price)
                .HasColumnType("decimal");
    }
}