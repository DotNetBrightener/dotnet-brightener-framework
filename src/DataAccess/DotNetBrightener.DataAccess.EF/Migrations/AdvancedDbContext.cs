#nullable enable
using DotNetBrightener.DataAccess.EF.Converters;
using DotNetBrightener.DataAccess.EF.Internal;
using EntityFramework.Exceptions.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.DataAccess.EF.Migrations;

/// <summary>
///     Represents the <see cref="DbContext"/> that can define the entities and should have migrations applied
/// </summary>
public abstract class AdvancedDbContext(DbContextOptions options) : DbContext(options)
{
    [Injectable]
    protected IServiceProvider? ServiceProvider;

    protected List<IDbContextConfigurator>? Configurators => ServiceProvider?.GetServices<IDbContextConfigurator>()
                                                                             .ToList();

    protected List<IDbContextConventionConfigurator>? ConventionConfigurators => ServiceProvider
                                                                               ?.GetServices<
                                                                                     IDbContextConventionConfigurator>()
                                                                                .ToList();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseExceptionProcessor();

        if (Configurators is { Count: > 0 })
        {
            foreach (var configurator in Configurators)
            {
                configurator.OnConfiguring(optionsBuilder);
            }
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        base.ConfigureConventions(builder);

        // making sure we'll be storing UTC time always
        builder.Properties<DateTimeOffset>()
               .HaveConversion<DateTimeOffsetUtcConverter>();

        if (ConventionConfigurators is { Count: > 0 })
        {
            foreach (var configurator in ConventionConfigurators)
            {
                configurator.ConfigureConventions(this, builder);
            }
        }
    }
}