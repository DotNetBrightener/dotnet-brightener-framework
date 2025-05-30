using DotNetBrightener.MultiTenancy.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.MultiTenancy.DbContexts;

public static class DbContextMultiTenantExtensions
{
    /// <summary>
    ///     Enables the entity for supporting Multi-tenant feature
    /// </summary>
    /// <param name="modelBuilder">
    ///     The <see cref="ModelBuilder"/> instance to attach entities for supporting multi-tenant
    /// </param>
    public static void EnableMultiTenantSupport(this ModelBuilder modelBuilder)
    {
        var tenant              = modelBuilder.Entity<Tenant>();
        var tenantEntityMapping = modelBuilder.Entity<TenantEntityMapping>();

        tenant.HasIndex(t => t.TenantGuid);

        tenantEntityMapping.HasKey(m => m.Id);

        tenantEntityMapping.HasIndex(m => new
                            {
                                m.EntityId,
                                m.EntityType,
                                m.TenantId
                            })
                           .IsUnique();

        tenantEntityMapping.HasOne<Tenant>()
                           .WithMany()
                           .HasForeignKey(m => m.TenantId);
    }
}