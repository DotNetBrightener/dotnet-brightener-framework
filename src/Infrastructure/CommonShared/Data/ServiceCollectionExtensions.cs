using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.EF.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.CommonShared.Data;

public static class ServiceCollectionExtensions
{
    public static void AddCentralizedDatabaseModule<TMainDbContext, TDbContext>(
        this IServiceCollection serviceCollection,
        DatabaseConfiguration   dbConfiguration)
        where TDbContext : TMainDbContext, IMigrationDefinitionDbContext<TMainDbContext>
        where TMainDbContext : DbContext
    {
        serviceCollection.AddEntityFrameworkDataServices<TMainDbContext>(dbConfiguration);

        serviceCollection.AddDbContext<TDbContext>();
        serviceCollection.RegisterStartupTask<AutoMigrateDbStartupTask<TDbContext>>();
    }
}