using System;
using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.EF.Extensions;
using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.WebApp.CommonShared.Data;

public static class ServiceCollectionExtensions
{
    public static void AddCentralizedDatabaseModule<TMainDbContext, TDbContext>(
        this IServiceCollection serviceCollection,
        DatabaseConfiguration   dbConfiguration,
        Action<DbContextOptionsBuilder> configureAction = null)
        where TDbContext : TMainDbContext, IMigrationDefinitionDbContext<TMainDbContext>
        where TMainDbContext : DbContext
    {
        serviceCollection.AddEntityFrameworkDataServices<TMainDbContext>(dbConfiguration, configureAction);

        serviceCollection.AddDbContext<TDbContext>();

        serviceCollection.RegisterStartupTask<AutoMigrateDbStartupTask<TDbContext>>();
    }
}