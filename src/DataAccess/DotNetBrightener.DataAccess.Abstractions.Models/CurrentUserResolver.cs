using Microsoft.AspNetCore.Http;

namespace DotNetBrightener.DataAccess;

/// <summary>
///     Defines the contracts of how to identify the current logged user who executes the database operation, and obtain username and id
/// </summary>
public interface ICurrentLoggedInUserResolver
{
    string CurrentUserName { get; }

    string CurrentUserId { get; }
}

public class DefaultCurrentUserResolver : ICurrentLoggedInUserResolver
{
    private readonly IHttpContextAccessor      _httpContextAccessor;
    private readonly ScopedCurrentUserResolver _scopedCurrentUserResolver;

    public DefaultCurrentUserResolver(IHttpContextAccessor      httpContextAccessor,
                                      ScopedCurrentUserResolver scopedCurrentUserResolver)
    {
        _httpContextAccessor       = httpContextAccessor;
        _scopedCurrentUserResolver = scopedCurrentUserResolver;
    }

    public string CurrentUserName => _scopedCurrentUserResolver.CurrentUserName ??
                                     _httpContextAccessor.GetCurrentUserName();

    public string CurrentUserId => _scopedCurrentUserResolver.CurrentUserId ??
                                   _httpContextAccessor.GetCurrentUserId<string>();
}


public sealed class ScopedCurrentUserResolver
{
    public string CurrentUserName { get; internal set; }

    public string CurrentUserId { get; internal set; }

    /// <summary>
    ///     Initiates a new scoped that resolves the name of the current user to the given <see cref="name"/>
    /// </summary>
    /// <param name="name">The name to use as current username who performed the actions within the returned scope</param>
    /// <returns>
    ///     An <see cref="IDisposable"/> object that can be disposed later after the scope is no longer needed.
    ///     When the <see cref="ICurrentLoggedInUserResolver"/> is asked for the current username within this scope, it will return the given <see cref="name"/>
    /// </returns>
    public IDisposable StartUseNameScope(string name)
    {
        return new BackgroundNameSetter(this, name);
    }

    private class BackgroundNameSetter : IDisposable
    {
        private readonly ScopedCurrentUserResolver _scopedCurrentUserResolver;

        public BackgroundNameSetter(ScopedCurrentUserResolver scopedCurrentUserResolver, string name)
        {
            _scopedCurrentUserResolver                 = scopedCurrentUserResolver;
            _scopedCurrentUserResolver.CurrentUserName = name;
        }

        public void Dispose()
        {
            _scopedCurrentUserResolver.CurrentUserName = null;
        }
    }
}