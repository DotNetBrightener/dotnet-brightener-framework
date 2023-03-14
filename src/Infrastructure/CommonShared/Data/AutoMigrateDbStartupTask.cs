using System.Threading.Tasks;
using DotNetBrightener.Core.StartupTask;
using DotNetBrightener.DataAccess.EF.Extensions;
using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.WebApp.CommonShared.Data;

public class AutoMigrateDbStartupTask<TDbContext> : IStartupTask
    where TDbContext : DbContext, IMigrationDefinitionDbContext
{
    private readonly TDbContext _dbContext;

    public AutoMigrateDbStartupTask(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public int Order => 0;

    public Task Execute()
    {
        _dbContext.AutoMigrateDbSchema();

        return Task.CompletedTask;
    }
}