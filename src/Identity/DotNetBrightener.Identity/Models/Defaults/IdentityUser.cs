namespace DotNetBrightener.Identity.Models.Defaults;

/// <summary>
///     Default concrete implementation of the User entity.
///     This class provides a ready-to-use implementation of the abstract User base class
///     without any additional properties. Consumer applications can use this directly
///     or create their own custom user class by inheriting from the abstract User base class.
/// </summary>
public class IdentityUser : User
{
    /// <summary>
    ///     Initializes a new instance of the IdentityUser class.
    /// </summary>
    public IdentityUser()
    {
        // Base class constructor will be called automatically
        // All default initialization is handled by the abstract User base class
    }

    /// <summary>
    ///     Initializes a new instance of the IdentityUser class with the specified username.
    /// </summary>
    /// <param name="userName">The username for the user</param>
    public IdentityUser(string userName) : this()
    {
        UserName = userName;
        NormalizedUserName = userName?.ToUpperInvariant();
    }
}
