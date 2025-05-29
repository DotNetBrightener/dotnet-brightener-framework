using DotNetBrightener.DataAccess.EF.Migrations;
using DotNetBrightener.SiteSettings.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotNetBrightener.SiteSettings.Data.PostgreSql.Data;

internal class PostgreSqlDbContextDesignTimeFactory : PostgreSqlDbContextDesignTimeFactory<PostgreSqlStorageSiteSettingDbContext>;

internal class PostgreSqlStorageSiteSettingDbContext(DbContextOptions<PostgreSqlStorageSiteSettingDbContext> options)
    : AdvancedDbContext(options)
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