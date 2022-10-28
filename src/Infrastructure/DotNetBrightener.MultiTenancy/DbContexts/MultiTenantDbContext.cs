using DotNetBrightener.MultiTenancy.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.MultiTenancy.DbContexts;

public class MultiTenantDbContext : DbContext
{
    public MultiTenantDbContext(DbContextOptions<MultiTenantDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureModel(modelBuilder);
    }

    public static void ConfigureModel(ModelBuilder modelBuilder)
    {
        var tenant              = modelBuilder.Entity<Tenant>();
        var tenantEntityMapping = modelBuilder.Entity<TenantEntityMapping>();

        tenantEntityMapping
           .HasKey(_ => _.Id);

        tenantEntityMapping
           .HasIndex(_ => new
            {
                _.EntityId,
                _.EntityType,
                _.TenantId
            })
           .IsUnique();

        tenantEntityMapping.HasOne<Tenant>()
                           .WithMany()
                           .HasForeignKey(_ => _.TenantId);
    }
}