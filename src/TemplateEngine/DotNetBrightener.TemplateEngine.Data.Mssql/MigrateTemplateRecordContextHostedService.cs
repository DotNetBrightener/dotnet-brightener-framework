using DotNetBrightener.TemplateEngine.Data.Mssql.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.TemplateEngine.Data.Mssql;

internal class MigrateTemplateRecordContextHostedService : IHostedService, IDisposable
{
    private readonly IServiceScopeFactory                               _serviceScopeFactory;
    private readonly ILogger<MigrateTemplateRecordContextHostedService> _logger;
    private readonly IHostApplicationLifetime                           _lifetime;

    public MigrateTemplateRecordContextHostedService(IServiceScopeFactory serviceScopeFactory,
                                                     ILogger<MigrateTemplateRecordContextHostedService> logger,
                                                     IHostApplicationLifetime lifetime)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger              = logger;
        _lifetime            = lifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _lifetime.ApplicationStarted.Register(InitializeAfterAppStarted);

        return Task.CompletedTask;
    }

    private void InitializeAfterAppStarted()
    {
        using var scope     = _serviceScopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<TemplateEngineDbContext>();

        var pendingMigrations = dbContext.Database
                                         .GetPendingMigrations()
                                         .ToArray();

        if (!pendingMigrations.Any())
            return;

        _logger.LogInformation("Migrating database for {dbContextName}", dbContext.GetType().Name);

        dbContext.Database.Migrate();

        _logger.LogInformation("Database migration completed for {dbContextName}", dbContext.GetType().Name);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }

    public void Dispose()
    {
    }
}