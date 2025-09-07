namespace DotNetBrightener.Identity.Models.Defaults;

/// <summary>
///     Default concrete implementation of the Role entity.
///     This class provides a ready-to-use implementation of the abstract Role base class
///     without any additional properties. Consumer applications can use this directly
///     or create their own custom role class by inheriting from the abstract Role base class.
/// </summary>
public class IdentityRole : Role
{
    /// <summary>
    ///     Initializes a new instance of the IdentityRole class.
    /// </summary>
    public IdentityRole()
    {
        // Base class constructor will be called automatically
        // All default initialization is handled by the abstract Role base class
    }

    /// <summary>
    ///     Initializes a new instance of the IdentityRole class with the specified role name.
    /// </summary>
    /// <param name="roleName">The name of the role</param>
    public IdentityRole(string roleName) : this()
    {
        Name = roleName;
        NormalizedName = roleName?.ToUpperInvariant();
    }
}
