using DotNetBrightener.TemplateEngine.Data.PostgreSql.Data;
using DotNetBrightener.TemplateEngine.Data.PostgreSql.Services;
using DotNetBrightener.TemplateEngine.Data.Services;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class TemplateEngineStorageServiceCollectionExtensions
{
    public static IServiceCollection AddTemplateEnginePostgreSqlStorage(this IServiceCollection serviceCollection,
                                                                        string                  connectionString)
    {
        serviceCollection.AddDbContext<TemplateEngineDbContext>((optionBuilder) =>
        {
            optionBuilder.UseNpgsql(connectionString,
                                    contextOptionsBuilder =>
                                    {
                                        contextOptionsBuilder
                                           .MigrationsHistoryTable("__MigrationsHistory",
                                                                   TemplateEngineDbContext.SchemaName);
                                    })
                         .UseLazyLoadingProxies();
        });

        serviceCollection.AddScoped<TemplateEngineRepository>();

        serviceCollection.Replace(ServiceDescriptor
                                     .Scoped<ITemplateRegistrationService, PostgreSqlTemplateRegistrationService>());
        serviceCollection.Replace(ServiceDescriptor.Scoped<ITemplateStorageService, PosgreSqlTemplateStorageService>());
        
        serviceCollection.AddScoped<ITemplateRecordDataService, InternalTemplateRecordDataService>();

        serviceCollection.AddAutoMigrationForDbContextAfterAppStarted<TemplateEngineDbContext>();

        LinqToDBForEFTools.Initialize();

        return serviceCollection;
    }
}