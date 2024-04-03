using DotNetBrightener.DataAccess.EF.Converters;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.EF.Migrations;

/// <summary>
///     Represents the <see cref="DbContext"/> that can define the entities and should have migrations applied
/// </summary>
public abstract class MigrationEnabledDbContext : DbContext
{
    protected static Action<DbContextOptionsBuilder> OptionsBuilder { get; private set; }

    protected MigrationEnabledDbContext(DbContextOptions options)
        : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        OptionsBuilder?.Invoke(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var derivedDbContextAssembly = GetType().Assembly;
        modelBuilder.ApplyConfigurationsFromAssembly(derivedDbContextAssembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        base.ConfigureConventions(builder);
        builder.Properties<DateOnly>()
               .HaveConversion<DateOnlyConverter>();
                
        builder.Properties<TimeOnly>()
               .HaveConversion<TimeOnlyConverter>();
    }

    protected void SetConfigureDbOptionsBuilder(Action<DbContextOptionsBuilder> optionsBuilder)
    {
        if (OptionsBuilder == null &&
            optionsBuilder != null)
            OptionsBuilder = optionsBuilder;
    }
}