using DotNetBrightener.SiteSettings.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.SiteSettings.DbContexts;

public class SiteSettingDbContext(DbContextOptions<SiteSettingDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureModel(modelBuilder);
    }

    public static void ConfigureModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SiteSettingRecord>();
    }
}