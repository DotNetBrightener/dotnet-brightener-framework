using System.Threading.Tasks;
using DotNetBrightener.Core.StartupTask;
using DotNetBrightener.DataAccess.EF.Extensions;
using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.WebApp.CommonShared.Data;

public class AutoMigrateDbStartupTask<TDbContext> : ISynchronousStartupTask
    where TDbContext : DbContext, IMigrationDefinitionDbContext
{
    private readonly TDbContext _dbContext;
    private readonly ILogger    _logger;

    public AutoMigrateDbStartupTask(TDbContext dbContext, 
                                    ILogger<AutoMigrateDbStartupTask<TDbContext>> logger)
    {
        _dbContext = dbContext;
        _logger    = logger;
    }

    public int Order => 0;

    public Task Execute()
    {
        _dbContext.AutoMigrateDbSchema();

        return Task.CompletedTask;
    }
}