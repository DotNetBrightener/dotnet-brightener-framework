using DotNetBrightener.MultiTenancy.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.MultiTenancy.DbContexts;

public abstract class MultiTenantEnabledDbContext<TTenantEntity> : DbContext
    where TTenantEntity : TenantBase, new()
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.EnableMultiTenantSupport<TTenantEntity>();
    }
}

public static class DbContextMultiTenantExtensions
{
    /// <summary>
    ///     Enables the entity for supporting Multi-tenant feature
    /// </summary>
    /// <param name="modelBuilder">
    ///     The <see cref="ModelBuilder"/> instance to attach entities for supporting multi-tenant
    /// </param>
    public static void EnableMultiTenantSupport<TTenantEntity>(this ModelBuilder modelBuilder)
        where TTenantEntity : TenantBase, new()
    {
        var tenant              = modelBuilder.Entity<TTenantEntity>();
        var tenantEntityMapping = modelBuilder.Entity<TenantEntityMapping>();

        tenant.HasIndex(t => t.Name)
              .IsUnique();

        tenantEntityMapping.HasKey(m => m.Id);

        tenantEntityMapping.HasIndex(m => new
                            {
                                m.EntityId,
                                m.EntityType,
                                m.TenantId
                            })
                           .IsUnique();

        tenantEntityMapping.HasOne<TTenantEntity>()
                           .WithMany()
                           .HasForeignKey(m => m.TenantId);
    }
}