namespace DotNetBrightener.DataAccess;

/// <summary>
///     Defines the contracts of how to identify the current logged user who executes the database operation, and obtain user name and id
/// </summary>
public interface ICurrentLoggedInUserResolver
{
    string CurrentUserName { get; }

    string CurrentUserId { get; }
}

public class DefaultCurrentUserResolver : ICurrentLoggedInUserResolver
{
    public string CurrentUserName => null;

    public string CurrentUserId => null;
}