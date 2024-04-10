using DotNetBrightener.DataAccess.EF.Converters;
using DotNetBrightener.DataAccess.EF.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.EF.Migrations;

/// <summary>
///     Represents the <see cref="DbContext"/> that can define the entities and should have migrations applied
/// </summary>
public abstract class MigrationEnabledDbContext : DbContext
{
    protected static Action<DbContextOptionsBuilder> OptionsBuilder { get; private set; }

    protected List<Action<ModelConfigurationBuilder>> ConfigureConverter { get; } = new();

    protected MigrationEnabledDbContext(DbContextOptions options)
        : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
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

    /// <summary>
    ///     Registers to the <see cref="ModelBuilder"/> the lookup table for the specified <typeparamref name="TEnum"/>
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum to generate the lookup table</typeparam>
    /// <param name="modelBuilder">
    ///     The <see cref="ModelBuilder"/>
    /// </param>
    protected void RegisterEnumLookupTable<TEnum>(ModelBuilder modelBuilder)
        where TEnum : struct, Enum
    {
        var enumLookupTableName = typeof(TEnum).Name + "Lookup";

        modelBuilder.Entity<EnumLookupEntity<TEnum>>()
                    .ToTable(enumLookupTableName)
                    .HasData(Enum.GetValues<TEnum>()
                                 .Select(x => new EnumLookupEntity<TEnum>
                                  {
                                      Id    = Convert.ToInt32(x),
                                      Value = x.ToString()
                                  }));

        ConfigureConverter.Add(builder =>
        {
            builder.Properties<TEnum>()
                   .HaveConversion<EnumLookupConverter<TEnum>>();
        });
    }

    protected void SetConfigureDbOptionsBuilder(Action<DbContextOptionsBuilder> optionsBuilder)
    {
        if (OptionsBuilder == null &&
            optionsBuilder != null)
            OptionsBuilder = optionsBuilder;
    }
}