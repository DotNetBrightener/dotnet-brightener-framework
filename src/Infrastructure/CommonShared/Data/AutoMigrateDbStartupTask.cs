using DotNetBrightener.Core.StartupTask;
using DotNetBrightener.DataAccess.EF.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DotNetBrightener.CommonShared.Data;

public class AutoMigrateDbStartupTask<TDbContext> : IStartupTask
    where TDbContext : DbContext
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