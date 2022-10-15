using System;
using DotNetBrightener.DataAccess.EF.Repositories;
using DotNetBrightener.DataAccess.Services;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.DataAccess.EF.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddEFDataServices<TDbContext>(this IServiceCollection serviceCollection,
                                                     DatabaseConfiguration   dbConfiguration,
                                                     Action<DbContextOptionsBuilder> configureAction = null)
        where TDbContext : DbContext
    {
        serviceCollection.AddSingleton(dbConfiguration);


        serviceCollection.AddDbContext<TDbContext>(configure =>
        {
            configureAction?.Invoke(configure);
            configure.UseLazyLoadingProxies();
        });

        // register QuizMeDbContext as DbContext so it can be picked up and used by EfRepository;
        serviceCollection.AddScoped<DbContext, TDbContext>();

        serviceCollection.AddScoped<IRepository, Repository>();
        LinqToDBForEFTools.Initialize();
    }
}