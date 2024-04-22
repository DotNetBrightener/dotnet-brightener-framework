using DotNetBrightener.DataAccess.EF.Migrations;
using LocaleManagement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocaleManagement.Database.DbContexts;

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
        modelBuilder.ApplyConfigurationsFromCurrentAssembly();
    }
}

public class AppLocaleDictionaryEntityConfiguration: IEntityTypeConfiguration<AppLocaleDictionary>
{
    public void Configure(EntityTypeBuilder<AppLocaleDictionary> builder)
    {
        
    }
}

public class DictionaryEntryEntityConfiguration : IEntityTypeConfiguration<DictionaryEntry>
{
    public void Configure(EntityTypeBuilder<DictionaryEntry> builder)
    {
        
    }
}