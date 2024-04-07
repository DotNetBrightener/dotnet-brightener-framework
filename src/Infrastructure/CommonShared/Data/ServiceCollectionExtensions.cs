using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebApp.CommonShared.Data;

public static class ServiceCollectionExtensions
{
    public static void AddCentralizedDatabaseModule<TMainDbContext, TMigrationDefinitionDbContext>(
        this IServiceCollection         serviceCollection,
        DatabaseConfiguration           dbConfiguration,
        IConfiguration                  configuration,
        Action<DbContextOptionsBuilder> configureAction = null)
        where TMigrationDefinitionDbContext : TMainDbContext, IMigrationDefinitionDbContext<TMainDbContext>
        where TMainDbContext : DbContext
    {
        serviceCollection.AddEntityFrameworkDataServices<TMainDbContext>(dbConfiguration,
                                                                         configuration,
                                                                         configureAction);

        serviceCollection.UseMigrationDbContext<TMigrationDefinitionDbContext, TMainDbContext>();
    }
}