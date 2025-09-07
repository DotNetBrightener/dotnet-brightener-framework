using DotNetBrightener.Identity.Models;
using DotNetBrightener.Identity.Models.Defaults;

namespace DotNetBrightener.Identity.Services;

/// <summary>
///     Generic interface for user password management operations
/// </summary>
/// <typeparam name="TUser">The type of user entity</typeparam>
public interface IUserPasswordManager<TUser> where TUser : User
{
    /// <summary>
    ///     Sets a password for a user
    /// </summary>
    Task<IdentityResult> SetPasswordAsync(TUser user, string password, string? reason = null, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    ///     Changes a user's password
    /// </summary>
    Task<IdentityResult> ChangePasswordAsync(TUser user, string currentPassword, string newPassword, string? reason = null, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    ///     Resets a user's password
    /// </summary>
    Task<IdentityResult> ResetPasswordAsync(TUser user, string token, string newPassword, string? reason = null, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    ///     Generates a password reset token
    /// </summary>
    Task<string> GeneratePasswordResetTokenAsync(TUser user);

    /// <summary>
    ///     Validates a password reset token
    /// </summary>
    Task<bool> VerifyPasswordResetTokenAsync(TUser user, string token);

    /// <summary>
    ///     Checks if a password is valid for a user
    /// </summary>
    Task<bool> CheckPasswordAsync(TUser user, string password);

    /// <summary>
    ///     Gets the current password for a user
    /// </summary>
    Task<UserPassword?> GetCurrentPasswordAsync(TUser user);

    /// <summary>
    ///     Gets password history for a user
    /// </summary>
    Task<IList<UserPasswordHistory>> GetPasswordHistoryAsync(TUser user, int count = 10);

    /// <summary>
    ///     Checks if a password has been used before
    /// </summary>
    Task<bool> HasPasswordBeenUsedAsync(TUser user, string password, int historyCount = 5);

    /// <summary>
    ///     Forces a user to change their password on next login
    /// </summary>
    Task<IdentityResult> RequirePasswordChangeAsync(TUser user, string? reason = null);

    /// <summary>
    ///     Validates a password against policy
    /// </summary>
    Task<IdentityResult> ValidatePasswordAsync(TUser user, string password);

    /// <summary>
    ///     Hashes a password
    /// </summary>
    string HashPassword(TUser user, string password);

    /// <summary>
    ///     Verifies a hashed password
    /// </summary>
    bool VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword);

    /// <summary>
    ///     Generates a security stamp
    /// </summary>
    string GenerateSecurityStamp();

    /// <summary>
    ///     Checks if a user's password is expired
    /// </summary>
    Task<bool> IsPasswordExpiredAsync(TUser user);

    /// <summary>
    ///     Gets password expiration date for a user
    /// </summary>
    Task<DateTimeOffset?> GetPasswordExpirationAsync(TUser user);

    /// <summary>
    ///     Sets password expiration for a user
    /// </summary>
    Task<IdentityResult> SetPasswordExpirationAsync(TUser user, DateTimeOffset? expirationDate);
}

/// <summary>
///     Non-generic interface for user password management operations using default IdentityUser
/// </summary>
public interface IUserPasswordManager : IUserPasswordManager<IdentityUser>
{
}
