﻿using DotNetBrightener.DataAccess.EF.Converters;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.EF.Migrations;

/// <summary>
///     Represents an extended <see cref="DbContext"/> that extends the conventions configuration
/// </summary>
public interface IExtendedConventionsDbContext
{
    List<Action<ModelConfigurationBuilder>> ConventionConfigureActions { get; }
}

/// <summary>
///     Represents the <see cref="DbContext"/> that can define the entities and should have migrations applied
/// </summary>
public abstract class MigrationEnabledDbContext : DbContext, IExtendedConventionsDbContext
{
    protected static Action<DbContextOptionsBuilder> OptionsBuilder { get; private set; }

    public List<Action<ModelConfigurationBuilder>> ConventionConfigureActions { get; } = new();
    
    protected MigrationEnabledDbContext(DbContextOptions options)
        : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        OptionsBuilder?.Invoke(optionsBuilder);
    }
    
    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        base.ConfigureConventions(builder);

        builder.Properties<DateOnly>()
               .HaveConversion<DateOnlyConverter>();

        builder.Properties<TimeOnly>()
               .HaveConversion<TimeOnlyConverter>();

        this.ExtendConfigureConventions(builder);
    }

    protected void SetConfigureDbOptionsBuilder(Action<DbContextOptionsBuilder> optionsBuilder)
    {
        if (OptionsBuilder == null &&
            optionsBuilder != null)
            OptionsBuilder = optionsBuilder;
    }
}
