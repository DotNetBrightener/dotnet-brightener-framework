using DotNetBrightener.DataAccess.Auditing.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotNetBrightener.DataAccess.Auditing.Storage.DbContexts;

internal class SqlServerDbContextDesignTimeFactory : SqlServerDbContextDesignTimeFactory<MssqlStorageAuditingDbContext> { }

internal class MssqlStorageAuditingDbContext(DbContextOptions<MssqlStorageAuditingDbContext> options)
    : DbContext(options)
{
    internal const string SchemaName = "Auditing";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var templateRecordEntity = modelBuilder.Entity<AuditEntity>();

        templateRecordEntity.ToTable(nameof(AuditEntity), schema: SchemaName);
    }
}