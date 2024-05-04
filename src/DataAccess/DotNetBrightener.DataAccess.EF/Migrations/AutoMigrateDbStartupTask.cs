using DotNetBrightener.DataAccess.EF.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.DataAccess.EF.Migrations;

internal class AutoMigrateDbStartupTask<TDbContext>(
    IServiceScopeFactory                          serviceScopeFactory,
    ILogger<AutoMigrateDbStartupTask<TDbContext>> logger,
    IOptions<DataMigrationOptions>                migrationOptions)
    : MigrateDbContextAtStartup<TDbContext>(serviceScopeFactory, logger)
    where TDbContext : DbContext, IMigrationDefinitionDbContext
{
    private readonly DataMigrationOptions _migrationOptions = migrationOptions.Value;

    protected override bool ShouldRun()
    {
        return _migrationOptions.AutoMigrateDatabase;
    }
}