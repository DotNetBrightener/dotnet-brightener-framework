using System;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.EF.Migrations;

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