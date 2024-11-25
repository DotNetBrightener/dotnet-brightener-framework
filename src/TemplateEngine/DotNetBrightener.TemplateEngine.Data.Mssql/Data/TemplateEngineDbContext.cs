using DotNetBrightener.DataAccess.EF.Migrations;
using DotNetBrightener.TemplateEngine.Data.Mssql.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotNetBrightener.TemplateEngine.Data.Mssql.Data;

internal class SqlServerDbContextDesignTimeFactory : SqlServerDbContextDesignTimeFactory<TemplateEngineDbContext>;

public class TemplateEngineDbContext(
    DbContextOptions<TemplateEngineDbContext> options)
    : AdvancedDbContext(options)
{
    internal const string SchemaName = "TemplateEngine";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TemplateRecord>(templateRecordEntity =>
        {
            templateRecordEntity.ToTable(nameof(TemplateRecord), schema: SchemaName);

            templateRecordEntity.Property(_ => _.TemplateType)
                                .HasMaxLength(512);

            templateRecordEntity.HasIndex(x => x.TemplateType);
        });

    }
}