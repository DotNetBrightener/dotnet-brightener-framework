using DotNetBrightener.Identity.Models;
using DotNetBrightener.Identity.Models.Defaults;

namespace DotNetBrightener.Identity.Services;

/// <summary>
///     Generic interface for user management operations
/// </summary>
/// <typeparam name="TUser">The type of user entity</typeparam>
public interface IUserManager<TUser> where TUser : User
{
    /// <summary>
    ///     Creates a new user
    /// </summary>
    Task<IdentityResult> CreateAsync(TUser user);

    /// <summary>
    ///     Updates an existing user
    /// </summary>
    Task<IdentityResult> UpdateAsync(TUser user);

    /// <summary>
    ///     Deletes a user (soft delete)
    /// </summary>
    Task<IdentityResult> DeleteAsync(TUser user);

    /// <summary>
    ///     Finds a user by ID
    /// </summary>
    Task<TUser?> FindByIdAsync(string userId);

    /// <summary>
    ///     Finds a user by username
    /// </summary>
    Task<TUser?> FindByNameAsync(string userName);

    /// <summary>
    ///     Finds a user by email
    /// </summary>
    Task<TUser?> FindByEmailAsync(string email);

    /// <summary>
    ///     Gets all users
    /// </summary>
    Task<IList<TUser>> GetUsersAsync();

    /// <summary>
    ///     Gets users in a specific account
    /// </summary>
    Task<IList<TUser>> GetUsersInAccountAsync(Guid accountId);

    /// <summary>
    ///     Checks if a user exists
    /// </summary>
    Task<bool> UserExistsAsync(string userName);

    /// <summary>
    ///     Sets the email for a user
    /// </summary>
    Task<IdentityResult> SetEmailAsync(TUser user, string email);

    /// <summary>
    ///     Sets the phone number for a user
    /// </summary>
    Task<IdentityResult> SetPhoneNumberAsync(TUser user, string phoneNumber);

    /// <summary>
    ///     Confirms a user's email
    /// </summary>
    Task<IdentityResult> ConfirmEmailAsync(TUser user, string token);

    /// <summary>
    ///     Generates an email confirmation token
    /// </summary>
    Task<string> GenerateEmailConfirmationTokenAsync(TUser user);

    /// <summary>
    ///     Sets lockout end date for a user
    /// </summary>
    Task<IdentityResult> SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd);

    /// <summary>
    ///     Increments access failed count
    /// </summary>
    Task<IdentityResult> AccessFailedAsync(TUser user);

    /// <summary>
    ///     Resets access failed count
    /// </summary>
    Task<IdentityResult> ResetAccessFailedCountAsync(TUser user);

    /// <summary>
    ///     Checks if a user is locked out
    /// </summary>
    Task<bool> IsLockedOutAsync(TUser user);

    /// <summary>
    ///     Sets two-factor authentication enabled for a user
    /// </summary>
    Task<IdentityResult> SetTwoFactorEnabledAsync(TUser user, bool enabled);

    /// <summary>
    ///     Gets whether two-factor authentication is enabled for a user
    /// </summary>
    Task<bool> GetTwoFactorEnabledAsync(TUser user);
}

/// <summary>
///     Represents the result of an identity operation
/// </summary>
public class IdentityResult
{
    public bool Succeeded { get; set; }
    public IEnumerable<IdentityError> Errors { get; set; } = new List<IdentityError>();

    public static IdentityResult Success => new() { Succeeded = true };

    public static IdentityResult Failed(params IdentityError[] errors)
    {
        return new IdentityResult
        {
            Succeeded = false,
            Errors = errors
        };
    }

    public static IdentityResult Failed(params string[] errors)
    {
        return new IdentityResult
        {
            Succeeded = false,
            Errors = errors.Select(e => new IdentityError { Description = e })
        };
    }
}

/// <summary>
///     Represents an error that occurred during an identity operation
/// </summary>
public class IdentityError
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
///     Non-generic interface for user management operations using default IdentityUser
/// </summary>
public interface IUserManager : IUserManager<IdentityUser>
{
}
