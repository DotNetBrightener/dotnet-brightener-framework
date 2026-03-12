using DotNetBrightener.DataAccess;
using DotNetBrightener.TemplateEngine.Data.PostgreSql.Data;
using DotNetBrightener.TemplateEngine.Data.PostgreSql.Services;
using DotNetBrightener.TemplateEngine.Data.Services;
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
        
        serviceCollection.TryAddScoped<ScopedCurrentUserResolver>();

        serviceCollection.Replace(ServiceDescriptor.Scoped<ITemplateRegistrationService, PostgreSqlTemplateRegistrationService>());
        serviceCollection.Replace(ServiceDescriptor.Scoped<ITemplateStorageService, PosgreSqlTemplateStorageService>());

        serviceCollection.AddScoped<TemplateEngineRepository>();
        serviceCollection.AddScoped<ITemplateRecordDataService, InternalTemplateRecordDataService>();
        
        return serviceCollection;
    }
}