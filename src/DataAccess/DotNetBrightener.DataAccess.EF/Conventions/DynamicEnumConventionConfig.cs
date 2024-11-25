using DotNetBrightener.DataAccess.EF.EnumLookup;
using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.EF.Conventions;

internal class DynamicEnumConventionConfig(ILookupEnumContainer lookUpEnumContainer)
    : IDbContextConventionConfigurator
{
    public void ConfigureConventions(DbContext dbContext, ModelConfigurationBuilder builder)
    {
        lookUpEnumContainer.ConventionConfigureActions
                           .ForEach(a =>
                            {
                                a.Invoke(builder);
                            });
    }
}