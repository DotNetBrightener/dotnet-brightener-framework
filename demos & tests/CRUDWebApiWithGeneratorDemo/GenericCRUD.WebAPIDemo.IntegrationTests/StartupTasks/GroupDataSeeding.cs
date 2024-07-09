using Bogus;
using CRUDWebApiWithGeneratorDemo.Core.Entities;
using CRUDWebApiWithGeneratorDemo.Services.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GenericCRUD.WebAPIDemo.IntegrationTests.StartupTasks;

internal class GroupDataSeeding(
    IServiceScopeFactory     serviceScopeFactory,
    IHostApplicationLifetime lifetime,
    ILoggerFactory           loggerFactory)
    : DataSeedingStartupTask(serviceScopeFactory, lifetime, loggerFactory)
{

    protected override async Task Seed(IServiceProvider serviceProvider)
    {
        var groupEntityDataService = serviceProvider.GetRequiredService<IGroupEntityDataService>();

        var dataToSeed = new List<GroupEntity>();

        // seeding data for 5-6-7 of July, in Denver time zone

        for (int dayOfMonth = 5; dayOfMonth < 8; dayOfMonth++)
        {
            for (var hour = 0; hour < 24; hour++)
            {
                var createdDate = DateTimeOffset.Parse($"2024-07-{dayOfMonth:00}T{hour:00}:00:00.000-06:00");

                var groupEntity = new GroupEntity
                {
                    Name        = $"Work done on {createdDate:O}",
                    CreatedDate = createdDate.ToUniversalTime()
                };

                dataToSeed.Add(groupEntity);
            }
        }

        await groupEntityDataService.InsertManyAsync(dataToSeed);
    }
}