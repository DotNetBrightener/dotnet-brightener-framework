namespace DotNetBrightener.Core.DataAccess.Abstractions.Resolvers
{
    public interface ICurrentLoggedInUserResolver
    {
        string CurrentUserName { get; }

        string CurrentUserId { get; }
    }

    /// <summary>
    ///     Represents logic of how to identify the current user who executes the database operation
    /// </summary>
    public class DefaultCurrentUserResolver : ICurrentLoggedInUserResolver
    {
        public string CurrentUserName => null;

        public string CurrentUserId   => null;
    }
}
