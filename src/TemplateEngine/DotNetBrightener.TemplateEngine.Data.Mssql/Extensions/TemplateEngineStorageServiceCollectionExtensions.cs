using DotNetBrightener.DataAccess;
using DotNetBrightener.TemplateEngine.Data.Mssql.Data;
using DotNetBrightener.TemplateEngine.Data.Mssql.Services;
using DotNetBrightener.TemplateEngine.Data.Services;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class TemplateEngineStorageServiceCollectionExtensions
{
    public static IServiceCollection AddTemplateEngineSqlServerStorage(this IServiceCollection serviceCollection,
                                                                       string                  connectionString)
    {
        serviceCollection.AddDbContext<TemplateEngineDbContext>((optionBuilder) =>
        {
            optionBuilder.UseSqlServer(connectionString,
                                       contextOptionsBuilder =>
                                       {
                                           contextOptionsBuilder
                                              .MigrationsHistoryTable("__MigrationsHistory",
                                                                      TemplateEngineDbContext.SchemaName);
                                       })
                         .UseLazyLoadingProxies();
        });

        serviceCollection.TryAddScoped<ScopedCurrentUserResolver>();

        serviceCollection.Replace(ServiceDescriptor.Scoped<ITemplateRegistrationService, SqlServerTemplateRegistrationService>());
        serviceCollection.Replace(ServiceDescriptor.Scoped<ITemplateStorageService, SqlServerTemplateStorageService>());

        serviceCollection.AddScoped<TemplateEngineRepository>();
        serviceCollection.AddScoped<ITemplateRecordDataService, InternalTemplateRecordDataService>();

        LinqToDBForEFTools.Initialize();

        return serviceCollection;
    }
}