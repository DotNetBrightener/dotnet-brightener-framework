using DotNetBrightener.DataAccess.EF.Converters;
using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.EF.Conventions;

public class TimeOnlyConventionConfig : IDbContextConventionConfigurator
{
    public void ConfigureConventions(DbContext dbContext, ModelConfigurationBuilder builder)
    {
        builder.Properties<TimeOnly>()
               .HaveConversion<TimeOnlyConverter>();
    }
}