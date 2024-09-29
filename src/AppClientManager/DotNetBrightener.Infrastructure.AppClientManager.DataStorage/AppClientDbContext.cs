using DotNetBrightener.DataAccess.EF.Migrations;
using DotNetBrightener.Infrastructure.AppClientManager.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.Infrastructure.AppClientManager.DataStorage;

public class AppClientDbContext : AdvancedDbContext
{
    protected AppClientDbContext(DbContextOptions options)
        : base(options)
    {

    }

    protected AppClientDbContext(DbContextOptions                options,
                                 Action<DbContextOptionsBuilder> optionBuilder)
        : base(options)
    {
    }

    public AppClientDbContext(DbContextOptions<AppClientDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromCurrentAssembly();

        this.RegisterEnumLookupTable<AppClientStatus>(modelBuilder, schema: AppClientDataDefaults.AppClientSchemaName);

        this.RegisterEnumLookupTable<AppClientType>(modelBuilder, schema: AppClientDataDefaults.AppClientSchemaName);
    }
}