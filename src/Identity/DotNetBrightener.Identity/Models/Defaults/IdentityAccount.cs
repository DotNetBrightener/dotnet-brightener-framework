namespace DotNetBrightener.Identity.Models.Defaults;

/// <summary>
///     Default concrete implementation of the Account entity.
///     This class provides a ready-to-use implementation of the abstract Account base class
///     without any additional properties. Consumer applications can use this directly
///     or create their own custom account class by inheriting from the abstract Account base class.
/// </summary>
public class IdentityAccount : Account
{
    /// <summary>
    ///     Initializes a new instance of the IdentityAccount class.
    /// </summary>
    public IdentityAccount()
    {
        // Base class constructor will be called automatically
        // All default initialization is handled by the abstract Account base class
    }

    /// <summary>
    ///     Initializes a new instance of the IdentityAccount class with the specified account name.
    /// </summary>
    /// <param name="accountName">The name of the account</param>
    public IdentityAccount(string accountName) : this()
    {
        Name = accountName;
        DisplayName = accountName;
    }

    /// <summary>
    ///     Initializes a new instance of the IdentityAccount class with the specified account name and parent account.
    /// </summary>
    /// <param name="accountName">The name of the account</param>
    /// <param name="parentAccountId">The ID of the parent account for hierarchical structure</param>
    public IdentityAccount(string accountName, Guid? parentAccountId) : this(accountName)
    {
        ParentAccountId = parentAccountId;
    }
}
