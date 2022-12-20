using Microsoft.EntityFrameworkCore;
using System;

namespace DotNetBrightener.CommonShared.Data;

/// <summary>
///     Marks the implementation of this interface as the DbContext that provides migration definitions for its based <typeparamref name="TBaseDbContext"/>
/// </summary>
/// <typeparam name="TBaseDbContext">
///     The DbContext that defines all the models and entities
/// </typeparam>
public interface IMigrationDefinitionDbContext<TBaseDbContext> where TBaseDbContext : DbContext { }

/// <summary>
///     Represents the <see cref="DbContext"/> that can defines the entities and should have migrations applied
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

    protected void SetConfigureDbOptionsBuilder(Action<DbContextOptionsBuilder> optionsBuilder)
    {
        if (OptionsBuilder == null &&
            optionsBuilder != null)
            OptionsBuilder = optionsBuilder;
    }
}