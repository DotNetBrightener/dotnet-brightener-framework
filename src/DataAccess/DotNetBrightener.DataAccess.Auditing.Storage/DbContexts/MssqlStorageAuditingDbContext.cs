using DotNetBrightener.DataAccess.Auditing.Entities;
using EntityFramework.Exceptions.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotNetBrightener.DataAccess.Auditing.Storage.DbContexts;

internal class SqlServerDbContextDesignTimeFactory : SqlServerDbContextDesignTimeFactory<MssqlStorageAuditingDbContext> { }

internal class MssqlStorageAuditingDbContext(DbContextOptions<MssqlStorageAuditingDbContext> options)
    : DbContext(options)
{
    internal const string SchemaName = "Auditing";

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseExceptionProcessor();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditEntity>(auditEntity =>
        {
            auditEntity.ToTable(nameof(AuditEntity), schema: SchemaName);

            auditEntity.HasNoKey();

            auditEntity.HasIndex(audit => audit.Id);

            auditEntity.HasIndex(audit => new
            {
                audit.EntityType,
                audit.EntityIdentifier
            });

            auditEntity.HasIndex(audit => audit.StartTime);
        });
    }
}