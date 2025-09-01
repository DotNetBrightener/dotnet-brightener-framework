using DotNetBrightener.DataAccess.EF.Converters;
using DotNetBrightener.DataAccess.EF.Migrations;
using DotNetBrightener.IdentityAuth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public class IdentityAuthBuilder
{
    public IdentityBuilder    IdentityBuilder   { get; internal init; }
    public IServiceCollection ServiceCollection { get; internal init; }
}

public static class IdentityAuthServiceCollectionExtensions
{
    public static IdentityAuthBuilder AddIdentityAuth(this IServiceCollection services)
    {
        services.AddAuthentication()
                .AddBearerToken(IdentityConstants.BearerScheme);

        services.AddAuthorizationBuilder();

        var identityBuilder = services.AddIdentityCore<User>()
                                      .AddRoles<Role>()
                                      .AddApiEndpoints();

        var identityAuthBuilder = new IdentityAuthBuilder
        {
            IdentityBuilder   = identityBuilder,
            ServiceCollection = services
        };

        services.AddSingleton(identityAuthBuilder);

        return identityAuthBuilder;
    }

    /// <summary>
    ///     Adds the Entity Framework stores for the Identity
    /// </summary>
    /// <typeparam name="TDbContext">The Entity Framework database context to use</typeparam>
    /// <param name="builder">The <see cref="IdentityAuthBuilder"/> instance this method extends</param>
    /// <returns>The <see cref="IdentityAuthBuilder"/> instance this method extends</returns>
    public static IdentityAuthBuilder AddEntityFrameworkStores<TDbContext>(this IdentityAuthBuilder builder)
        where TDbContext : AuthIdentityBasedDbContext
    {
        builder.IdentityBuilder
               .AddEntityFrameworkStores<TDbContext>();

        return builder;
    }
}

public abstract class AuthIdentityBasedDbContext : IdentityDbContext<User, Role, Guid>, IExtendedConventionsDbContext
{
    protected AuthIdentityBasedDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        base.ConfigureConventions(builder);

        builder.Properties<DateOnly>()
               .HaveConversion<DateOnlyConverter>();

        builder.Properties<TimeOnly>()
               .HaveConversion<TimeOnlyConverter>();
    }

    public List<Action<ModelConfigurationBuilder>> ConventionConfigureActions { get; } = [];
}