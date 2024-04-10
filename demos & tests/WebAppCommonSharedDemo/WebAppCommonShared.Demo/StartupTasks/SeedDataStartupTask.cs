using DotNetBrightener.Core.StartupTask;
using DotNetBrightener.DataAccess.Services;
using Microsoft.EntityFrameworkCore;
using WebAppCommonShared.Demo.DbContexts;
using WebAppCommonShared.Demo.Entities;

namespace WebAppCommonShared.Demo.StartupTasks;

public class SeedDataStartupTask : IStartupTask
{
    private readonly MainDbContext _dbContext;
    private readonly IRepository   _repository;

    public SeedDataStartupTask(MainDbContext dbContext, IRepository repository)
    {
        _dbContext  = dbContext;
        _repository = repository;
    }

    public int Order => 100;

    public async Task Execute()
    {
        var existingSubscriptions = _repository.Count<Subscription>();

        if (existingSubscriptions > 0)
        {
            return;
        }

        var list                  = new List<Subscription>();

        for (var x = 0; x < 1000; x++)
        {
            var subscription = new Subscription
            {
                Name   = $"Subscription {x}",
                Status = GetRandomSubscriptionStatus(),
                UserId = x
            };

            list.Add(subscription);
        }

        await _repository.InsertMany(list);
    }

    private SubscriptionStatus GetRandomSubscriptionStatus()
    {
        var values = Enum.GetValues(typeof(SubscriptionStatus));
        var random = new Random();
        var randomStatus =
            (SubscriptionStatus)(values.GetValue(random.Next(values.Length)) ?? SubscriptionStatus.Invalid);

        return randomStatus;
    }
}