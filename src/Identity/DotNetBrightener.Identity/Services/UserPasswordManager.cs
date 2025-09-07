using System.Security.Cryptography;
using System.Text;
using DotNetBrightener.Identity.Models;
using DotNetBrightener.Identity.Models.Defaults;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Identity.Services;

/// <summary>
/// Generic implementation of user password management operations
/// </summary>
/// <typeparam name="TUser">The type of user entity</typeparam>
public class UserPasswordManager<TUser> : IUserPasswordManager<TUser> where TUser : User
{
    private readonly IdentityDbContext<TUser, IdentityRole, IdentityAccount> _context;
    private readonly ILogger<UserPasswordManager<TUser>> _logger;

    // Password policy settings - these could be configurable
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 128;
    private const int PasswordHistoryCount = 5;
    private const int DefaultPasswordExpirationDays = 90;

    public UserPasswordManager(
        IdentityDbContext<TUser, IdentityRole, IdentityAccount> context,
        ILogger<UserPasswordManager<TUser>> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IdentityResult> SetPasswordAsync(TUser user, string password, string? reason = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            if (user == null)
                return IdentityResult.Failed("User cannot be null");

            // Validate password
            var validationResult = await ValidatePasswordAsync(user, password);
            if (!validationResult.Succeeded)
                return validationResult;

            // Check if password has been used before
            if (await HasPasswordBeenUsedAsync(user, password))
                return IdentityResult.Failed("Password has been used recently and cannot be reused");

            // Deactivate current password
            var currentPassword = await GetCurrentPasswordAsync(user);
            if (currentPassword != null)
            {
                await DeactivatePasswordAsync(currentPassword, reason, ipAddress, userAgent);
            }

            // Create new password record
            var newPassword = new UserPassword
            {
                UserId = user.Id,
                PasswordHash = HashPassword(user, password),
                SecurityStamp = GenerateSecurityStamp(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                IsActive = true,
                PasswordSetAt = DateTimeOffset.UtcNow,
                PasswordExpiresAt = DateTimeOffset.UtcNow.AddDays(DefaultPasswordExpirationDays),
                MustChangePassword = false,
                PasswordChangeReason = reason ?? "Password set",
                PasswordChangeIpAddress = ipAddress,
                PasswordChangeUserAgent = userAgent,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedBy = "System" // TODO: Get current user context
            };

            _context.Set<UserPassword>().Add(newPassword);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Password set for user {UserId}", user.Id);
            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting password for user {UserId}", user?.Id);
            return IdentityResult.Failed("An error occurred while setting the password");
        }
    }

    public async Task<IdentityResult> ChangePasswordAsync(TUser user, string currentPassword, string newPassword, string? reason = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            if (user == null)
                return IdentityResult.Failed("User cannot be null");

            // Verify current password
            if (!await CheckPasswordAsync(user, currentPassword))
                return IdentityResult.Failed("Current password is incorrect");

            return await SetPasswordAsync(user, newPassword, reason ?? "Password changed", ipAddress, userAgent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", user?.Id);
            return IdentityResult.Failed("An error occurred while changing the password");
        }
    }

    public async Task<IdentityResult> ResetPasswordAsync(TUser user, string token, string newPassword, string? reason = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            if (user == null)
                return IdentityResult.Failed("User cannot be null");

            // Verify reset token
            if (!await VerifyPasswordResetTokenAsync(user, token))
                return IdentityResult.Failed("Invalid or expired password reset token");

            return await SetPasswordAsync(user, newPassword, reason ?? "Password reset", ipAddress, userAgent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user {UserId}", user?.Id);
            return IdentityResult.Failed("An error occurred while resetting the password");
        }
    }

    public async Task<string> GeneratePasswordResetTokenAsync(TUser user)
    {
        // TODO: Implement proper token generation with expiration
        // For now, return a simple token
        await Task.CompletedTask;
        return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user.Id}:{DateTimeOffset.UtcNow.AddHours(1).Ticks}"));
    }

    public async Task<bool> VerifyPasswordResetTokenAsync(TUser user, string token)
    {
        try
        {
            // TODO: Implement proper token verification
            // For now, simple verification
            await Task.CompletedTask;
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = decoded.Split(':');

            if (parts.Length != 2)
                return false;

            if (!Guid.TryParse(parts[0], out var userId) || userId != user.Id)
                return false;

            if (!long.TryParse(parts[1], out var expiration))
                return false;

            return new DateTimeOffset(expiration, TimeSpan.Zero) > DateTimeOffset.UtcNow;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CheckPasswordAsync(TUser user, string password)
    {
        try
        {
            if (user == null || string.IsNullOrEmpty(password))
                return false;

            var currentPassword = await GetCurrentPasswordAsync(user);
            if (currentPassword?.PasswordHash == null)
                return false;

            return VerifyHashedPassword(user, currentPassword.PasswordHash, password);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking password for user {UserId}", user?.Id);
            return false;
        }
    }

    public async Task<UserPassword?> GetCurrentPasswordAsync(TUser user)
    {
        if (user == null)
            return null;

        return await _context.Set<UserPassword>()
                           .Where(up => up.UserId == user.Id && up.IsActive && !up.IsDeleted)
                           .OrderByDescending(up => up.PasswordSetAt)
                           .FirstOrDefaultAsync();
    }

    public async Task<IList<UserPasswordHistory>> GetPasswordHistoryAsync(TUser user, int count = 10)
    {
        if (user == null)
            return new List<UserPasswordHistory>();

        return await _context.Set<UserPasswordHistory>()
                           .Where(uph => uph.UserId == user.Id && !uph.IsDeleted)
                           .OrderByDescending(uph => uph.PasswordChangedAt)
                           .Take(count)
                           .ToListAsync();
    }

    public async Task<bool> HasPasswordBeenUsedAsync(TUser user, string password, int historyCount = PasswordHistoryCount)
    {
        try
        {
            if (user == null || string.IsNullOrEmpty(password))
                return false;

            var hashedPassword = HashPassword(user, password);

            // Check current password
            var currentPassword = await GetCurrentPasswordAsync(user);
            if (currentPassword?.PasswordHash == hashedPassword)
                return true;

            // Check password history
            var history = await _context.Set<UserPasswordHistory>()
                                      .Where(uph => uph.UserId == user.Id && !uph.IsDeleted)
                                      .OrderByDescending(uph => uph.PasswordChangedAt)
                                      .Take(historyCount)
                                      .ToListAsync();

            return history.Any(h => h.PreviousPasswordHash == hashedPassword);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking password history for user {UserId}", user?.Id);
            return false;
        }
    }

    public async Task<IdentityResult> RequirePasswordChangeAsync(TUser user, string? reason = null)
    {
        try
        {
            if (user == null)
                return IdentityResult.Failed("User cannot be null");

            var currentPassword = await GetCurrentPasswordAsync(user);
            if (currentPassword == null)
                return IdentityResult.Failed("User has no current password");

            currentPassword.MustChangePassword = true;
            currentPassword.PasswordChangeReason = reason ?? "Password change required";
            currentPassword.ModifiedDate = DateTimeOffset.UtcNow;
            currentPassword.ModifiedBy = "System"; // TODO: Get current user context

            await _context.SaveChangesAsync();

            _logger.LogInformation("Password change required for user {UserId}", user.Id);
            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requiring password change for user {UserId}", user?.Id);
            return IdentityResult.Failed("An error occurred while requiring password change");
        }
    }

    public async Task<IdentityResult> ValidatePasswordAsync(TUser user, string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(password))
            errors.Add("Password is required");
        else
        {
            if (password.Length < MinPasswordLength)
                errors.Add($"Password must be at least {MinPasswordLength} characters long");

            if (password.Length > MaxPasswordLength)
                errors.Add($"Password cannot exceed {MaxPasswordLength} characters");

            if (!password.Any(char.IsUpper))
                errors.Add("Password must contain at least one uppercase letter");

            if (!password.Any(char.IsLower))
                errors.Add("Password must contain at least one lowercase letter");

            if (!password.Any(char.IsDigit))
                errors.Add("Password must contain at least one digit");

            if (!password.Any(c => !char.IsLetterOrDigit(c)))
                errors.Add("Password must contain at least one special character");

            // Check if password contains username
            if (!string.IsNullOrEmpty(user?.UserName) && 
                password.Contains(user.UserName, StringComparison.OrdinalIgnoreCase))
                errors.Add("Password cannot contain the username");
        }

        await Task.CompletedTask;

        return errors.Any() ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
    }

    public string HashPassword(TUser user, string password)
    {
        // Use PBKDF2 with SHA256
        const int saltSize = 16;
        const int hashSize = 32;
        const int iterations = 100000;

        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[saltSize];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(hashSize);

        // Combine salt and hash
        var hashBytes = new byte[saltSize + hashSize];
        Array.Copy(salt, 0, hashBytes, 0, saltSize);
        Array.Copy(hash, 0, hashBytes, saltSize, hashSize);

        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
    {
        try
        {
            const int saltSize = 16;
            const int hashSize = 32;
            const int iterations = 100000;

            var hashBytes = Convert.FromBase64String(hashedPassword);

            if (hashBytes.Length != saltSize + hashSize)
                return false;

            var salt = new byte[saltSize];
            Array.Copy(hashBytes, 0, salt, 0, saltSize);

            using var pbkdf2 = new Rfc2898DeriveBytes(providedPassword, salt, iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(hashSize);

            for (int i = 0; i < hashSize; i++)
            {
                if (hashBytes[i + saltSize] != hash[i])
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GenerateSecurityStamp()
    {
        return Guid.NewGuid().ToString();
    }

    public async Task<bool> IsPasswordExpiredAsync(TUser user)
    {
        try
        {
            var currentPassword = await GetCurrentPasswordAsync(user);
            if (currentPassword == null)
                return true;

            return currentPassword.PasswordExpiresAt.HasValue &&
                   currentPassword.PasswordExpiresAt <= DateTimeOffset.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking password expiration for user {UserId}", user?.Id);
            return true; // Assume expired on error for security
        }
    }

    public async Task<DateTimeOffset?> GetPasswordExpirationAsync(TUser user)
    {
        try
        {
            var currentPassword = await GetCurrentPasswordAsync(user);
            return currentPassword?.PasswordExpiresAt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting password expiration for user {UserId}", user?.Id);
            return null;
        }
    }

    public async Task<IdentityResult> SetPasswordExpirationAsync(TUser user, DateTimeOffset? expirationDate)
    {
        try
        {
            if (user == null)
                return IdentityResult.Failed("User cannot be null");

            var currentPassword = await GetCurrentPasswordAsync(user);
            if (currentPassword == null)
                return IdentityResult.Failed("User has no current password");

            currentPassword.PasswordExpiresAt = expirationDate;
            currentPassword.ModifiedDate = DateTimeOffset.UtcNow;
            currentPassword.ModifiedBy = "System"; // TODO: Get current user context

            await _context.SaveChangesAsync();

            _logger.LogInformation("Password expiration set for user {UserId}", user.Id);
            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting password expiration for user {UserId}", user?.Id);
            return IdentityResult.Failed("An error occurred while setting password expiration");
        }
    }

    private async Task DeactivatePasswordAsync(UserPassword password, string? reason, string? ipAddress, string? userAgent)
    {
        try
        {
            // Create history record
            var historyRecord = new UserPasswordHistory
            {
                UserPasswordId = password.Id,
                UserId = password.UserId,
                PreviousPasswordHash = password.PasswordHash,
                PreviousSecurityStamp = password.SecurityStamp,
                PasswordChangedAt = DateTimeOffset.UtcNow,
                ChangeReason = reason ?? "Password deactivated",
                ChangeIpAddress = ipAddress,
                ChangeUserAgent = userAgent,
                WasForcedChange = false,
                WasSecurityIncident = false,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedBy = "System" // TODO: Get current user context
            };

            _context.Set<UserPasswordHistory>().Add(historyRecord);

            // Deactivate current password
            password.IsActive = false;
            password.ModifiedDate = DateTimeOffset.UtcNow;
            password.ModifiedBy = "System"; // TODO: Get current user context

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating password for user {UserId}", password.UserId);
            throw;
        }
    }
}

/// <summary>
/// Non-generic implementation of user password management operations using default IdentityUser
/// </summary>
public class UserPasswordManager : UserPasswordManager<IdentityUser>, IUserPasswordManager
{
    public UserPasswordManager(
        IdentityDbContext context,
        ILogger<UserPasswordManager> logger)
        : base(context, logger)
    {
    }
}
