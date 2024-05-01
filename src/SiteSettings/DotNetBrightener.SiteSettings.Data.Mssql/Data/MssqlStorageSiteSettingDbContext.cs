using DotNetBrightener.DataAccess.EF.Migrations;
using DotNetBrightener.SiteSettings.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotNetBrightener.SiteSettings.Data.Mssql.Data;

internal class SqlServerDbContextDesignTimeFactory : SqlServerDbContextDesignTimeFactory<MssqlStorageSiteSettingDbContext> { }

internal class MssqlStorageSiteSettingDbContext(DbContextOptions<MssqlStorageSiteSettingDbContext> options)
    : MigrationEnabledDbContext(options)
{
    internal const string SchemaName = "SiteSettings";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var templateRecordEntity = modelBuilder.Entity<SiteSettingRecord>();

        templateRecordEntity.ToTable(nameof(SiteSettingRecord), schema: SchemaName);

        templateRecordEntity.Property(x => x.SettingType)
                            .HasMaxLength(1024);
    }
}