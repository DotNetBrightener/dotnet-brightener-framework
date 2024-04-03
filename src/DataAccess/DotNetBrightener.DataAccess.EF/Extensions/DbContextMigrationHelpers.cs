using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.DataAccess.EF.Extensions;

public static class DbContextMigrationHelpers
{
    /// <summary>
    ///     Processes the schema migration for the given <see cref="DbContext"/>
    /// </summary>
    /// <param name="dbMigrationContext">
    ///     The <see cref="DbContext"/> to detect and migrate the schema
    /// </param>
    /// <param name="logger">The <see cref="ILogger"/> instance for writing logs</param>
    public static void AutoMigrateDbSchema(this DbContext dbMigrationContext, ILogger logger = null)
    {
        logger?.LogInformation("Getting pending migrations for {dbContextName}",
                               dbMigrationContext.GetType().Name);

        var pendingMigrations = dbMigrationContext.Database
                                                  .GetPendingMigrations()
                                                  .ToArray();

        logger?.LogInformation("{dbContextName} has {numberOfMigrations} pending migrations",
                               dbMigrationContext.GetType().Name,
                               pendingMigrations.Length);

        if (pendingMigrations.Any())
        {
            logger?.LogInformation("Migrating database for {dbContextName}",
                                   dbMigrationContext.GetType().Name);

            try
            {
                var migrator = dbMigrationContext.Database.GetService<IMigrator>();

                foreach (var pendingMigration in pendingMigrations)
                {
                    logger?.LogInformation("Executing migration {pendingMigration}...", pendingMigration);

                    migrator.Migrate(pendingMigration);

                    logger?.LogInformation("Migration {pendingMigration} executed.", pendingMigration);
                }
            }
            catch (System.Exception exception)
            {
                logger?.LogError(exception,
                                 "Error while executing migration for context {dbContextName}",
                                 dbMigrationContext.GetType().Name);

                throw;
            }
        }
    }
}