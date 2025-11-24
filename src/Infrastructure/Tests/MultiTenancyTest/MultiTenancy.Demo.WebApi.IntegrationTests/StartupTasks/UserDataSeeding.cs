using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.MultiTenancy.Entities;
using DotNetBrightener.MultiTenancy.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MultiTenancy.Demo.WebApi.DbContexts;

namespace MultiTenancy.Demo.WebApi.IntegrationTests.StartupTasks;

internal class UserDataSeeding(
    IServiceScopeFactory     serviceScopeFactory,
    IHostApplicationLifetime lifetime,
    ILoggerFactory           loggerFactory)
    : DataSeedingStartupTask(serviceScopeFactory, lifetime, loggerFactory)
{
    private readonly IServiceScopeFactory _serviceScopeFactory1 = serviceScopeFactory;

    internal static Clinic Clinic1 = new()
    {
        Name    = "Clinic1",
        WhitelistedOrigins = "alloweddomain1.com",
        TenantDomains = "clinic1.com;clinic1.org;clinic1.net"
    };

    internal static Clinic Clinic2 = new()
    {
        Name               = "Clinic2",
        WhitelistedOrigins = "alloweddomain2.com",
        TenantDomains      = "clinic2.com;clinic2.org;clinic2.net"
    };

    protected override async Task Seed(IServiceProvider serviceProvider)
    {
        using (var scope = _serviceScopeFactory1.CreateScope())
        {
            var repository = scope.ServiceProvider.GetService<IRepository>();

            try
            {

                await using (repository.BeginUnitOfWork())
                {
                    await repository.InsertManyAsync(Clinic1, Clinic2);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        using (var scope = _serviceScopeFactory1.CreateScope())
        {
            var repository  = scope.ServiceProvider.GetService<IRepository>();
            var tenantScope = scope.ServiceProvider.GetService<ITenantAccessor>();

            using (tenantScope.UseTenant(Clinic1.Id))
            {
                try
                {
                    await using (repository.BeginUnitOfWork())
                    {
                        await repository.InsertManyAsync(new User
                                                         {
                                                             FirstName = "user 1",
                                                             LastName  = "clinic 1"
                                                         },
                                                         new User
                                                         {
                                                             FirstName = "user 2",
                                                             LastName  = "clinic 1"
                                                         });
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        using (var scope = _serviceScopeFactory1.CreateScope())
        {
            var repository  = scope.ServiceProvider.GetService<IRepository>();
            var tenantScope = scope.ServiceProvider.GetService<ITenantAccessor>();

            using (tenantScope.UseTenant(Clinic2.Id))
            {
                try
                {
                    await using (repository.BeginUnitOfWork())
                    {
                        await repository.InsertManyAsync(new User
                                                         {
                                                             FirstName = "user 1",
                                                             LastName  = "clinic 2"
                                                         },
                                                         new User
                                                         {
                                                             FirstName = "user 2",
                                                             LastName  = "clinic 2"
                                                         });
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
            }
        }

        await Task.Delay(TimeSpan.FromSeconds(2));

        using (var scope = _serviceScopeFactory1.CreateScope())
        {
            var repository = scope.ServiceProvider.GetService<MultiTenancyDbContext>();

            var clinics = await repository.Set<Clinic>()
                                          .ToArrayAsync();

            var users = await repository.Set<User>()
                                        .ToArrayAsync();

            var tenantMappings = await repository.Set<TenantEntityMapping>()
                                                 .ToArrayAsync();


        }
    }
}