using DotNetBrightener.Identity.Models;
using DotNetBrightener.Identity.Models.Defaults;
using DotNetBrightener.Identity.Services;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public class IdentityBuilder
{
    public IServiceCollection ServiceCollection { get; internal init; } = null!;
}

public static class IdentityServiceCollectionExtensions
{
    /// <summary>
    ///     Adds Identity services using default entity implementations
    /// </summary>
    public static IdentityBuilder AddIdentity(this IServiceCollection services)
    {
        return services.AddIdentity<IdentityUser, IdentityRole, IdentityAccount>();
    }

    /// <summary>
    ///     Adds Identity services using custom entity implementations
    /// </summary>
    /// <typeparam name="TUser">The type of user entity</typeparam>
    /// <typeparam name="TRole">The type of role entity</typeparam>
    /// <typeparam name="TAccount">The type of account entity</typeparam>
    public static IdentityBuilder AddIdentity<TUser, TRole, TAccount>(this IServiceCollection services)
        where TUser : User
        where TRole : Role
        where TAccount : Account
    {
        // Note: Authorization services should be added by the consuming application
        // services.AddAuthorization();

        // Register generic services
        services.AddScoped<IUserManager<TUser>, UserManager<TUser>>();
        services.AddScoped<IUserPasswordManager<TUser>, UserPasswordManager<TUser>>();

        // Register non-generic services for backward compatibility (only if using default types)
        if (typeof(TUser) == typeof(IdentityUser))
        {
            services.AddScoped<IUserManager>(provider => provider.GetRequiredService<IUserManager<TUser>>() as IUserManager ??
                throw new InvalidOperationException("Unable to resolve non-generic IUserManager"));
            services.AddScoped<UserManager>(provider => provider.GetRequiredService<UserManager<TUser>>() as UserManager ??
                throw new InvalidOperationException("Unable to resolve non-generic UserManager"));
        }

        if (typeof(TUser) == typeof(IdentityUser))
        {
            services.AddScoped<IUserPasswordManager>(provider => provider.GetRequiredService<IUserPasswordManager<TUser>>() as IUserPasswordManager ??
                throw new InvalidOperationException("Unable to resolve non-generic IUserPasswordManager"));
            services.AddScoped<UserPasswordManager>(provider => provider.GetRequiredService<UserPasswordManager<TUser>>() as UserPasswordManager ??
                throw new InvalidOperationException("Unable to resolve non-generic UserPasswordManager"));
        }

        var identityBuilder = new IdentityBuilder
        {
            ServiceCollection = services
        };

        services.AddSingleton(identityBuilder);

        return identityBuilder;
    }

    /// <summary>
    ///     Adds the Entity Framework stores for the Identity using the default IdentityDbContext
    /// </summary>
    /// <param name="builder">The <see cref="IdentityBuilder"/> instance this method extends</param>
    /// <param name="optionsAction">Action to configure the DbContext options</param>
    /// <returns>The <see cref="IdentityBuilder"/> instance this method extends</returns>
    public static IdentityBuilder AddEntityFrameworkStores(this IdentityBuilder builder,
        Action<DbContextOptionsBuilder>? optionsAction = null)
    {
        return builder.AddEntityFrameworkStores<IdentityDbContext>(optionsAction);
    }

    /// <summary>
    ///     Adds the Entity Framework stores for the Identity using a custom DbContext
    /// </summary>
    /// <typeparam name="TDbContext">The Entity Framework database context to use</typeparam>
    /// <param name="builder">The <see cref="IdentityBuilder"/> instance this method extends</param>
    /// <param name="optionsAction">Action to configure the DbContext options</param>
    /// <returns>The <see cref="IdentityBuilder"/> instance this method extends</returns>
    public static IdentityBuilder AddEntityFrameworkStores<TDbContext>(this IdentityBuilder builder,
        Action<DbContextOptionsBuilder>? optionsAction = null)
        where TDbContext : DbContext
    {
        builder.ServiceCollection.AddDbContext<TDbContext>(optionsAction ?? (_ => { }));

        // Register the generic DbContext as well for services that need it
        if (typeof(TDbContext) == typeof(IdentityDbContext))
        {
            // For the default non-generic DbContext, also register the generic version
            builder.ServiceCollection.AddScoped<IdentityDbContext<IdentityUser, IdentityRole, IdentityAccount>>(
                provider => provider.GetRequiredService<IdentityDbContext>());
        }

        return builder;
    }

    /// <summary>
    ///     Adds the Entity Framework stores for the Identity using a generic DbContext
    /// </summary>
    /// <typeparam name="TUser">The type of user entity</typeparam>
    /// <typeparam name="TRole">The type of role entity</typeparam>
    /// <typeparam name="TAccount">The type of account entity</typeparam>
    /// <param name="builder">The <see cref="IdentityBuilder"/> instance this method extends</param>
    /// <param name="optionsAction">Action to configure the DbContext options</param>
    /// <returns>The <see cref="IdentityBuilder"/> instance this method extends</returns>
    public static IdentityBuilder AddEntityFrameworkStores<TUser, TRole, TAccount>(this IdentityBuilder builder,
        Action<DbContextOptionsBuilder>? optionsAction = null)
        where TUser : User
        where TRole : Role
        where TAccount : Account
    {
        builder.ServiceCollection.AddDbContext<IdentityDbContext<TUser, TRole, TAccount>>(optionsAction ?? (_ => { }));

        return builder;
    }
}