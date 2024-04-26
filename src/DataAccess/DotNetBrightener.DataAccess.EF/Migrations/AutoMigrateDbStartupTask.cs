using DotNetBrightener.DataAccess.EF.Extensions;
using DotNetBrightener.DataAccess.EF.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.DataAccess.EF.Migrations;

public class AutoMigrateDbStartupTask<TDbContext> : IHostedService
    where TDbContext : DbContext, IMigrationDefinitionDbContext
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger              _logger;

    public AutoMigrateDbStartupTask(IServiceScopeFactory                          serviceScopeFactory,
                                    ILogger<AutoMigrateDbStartupTask<TDbContext>> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger              = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope  = _serviceScopeFactory.CreateScope();
        var       option = scope.ServiceProvider.GetRequiredService<IOptions<DataMigrationOptions>>();

        if (option?.Value.AutoMigrateDatabase != true)
            return Task.CompletedTask;

        using var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        dbContext.AutoMigrateDbSchema(_logger);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}