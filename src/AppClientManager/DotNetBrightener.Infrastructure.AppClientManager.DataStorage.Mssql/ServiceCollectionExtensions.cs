using DotNetBrightener.Infrastructure.AppClientManager;
using DotNetBrightener.Infrastructure.AppClientManager.DataStorage.Mssql;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable CheckNamespace

namespace Microsoft.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
    public static AppClientManagerBuilder WithMigrationUsingSqlServer(this AppClientManagerBuilder appClientManagerBuilder,
                                                                      string                       connectionString)
    {
        appClientManagerBuilder.Services.UseMigrationDbContext<AppClientDbMigrationContext>(option =>
        {
            option.UseSqlServer(connectionString);
        });

        return appClientManagerBuilder;
    }
}