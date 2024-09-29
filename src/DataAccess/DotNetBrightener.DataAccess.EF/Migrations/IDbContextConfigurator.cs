#nullable enable
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.EF.Migrations;

public interface IDbContextConfigurator
{
    void OnConfiguring(DbContextOptionsBuilder optionsBuilder);
}

public interface IDbContextConventionConfigurator
{
    void ConfigureConventions(DbContext dbContext, ModelConfigurationBuilder builder);
}