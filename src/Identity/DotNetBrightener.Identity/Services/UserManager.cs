using DotNetBrightener.Identity.Data;
using DotNetBrightener.Identity.Models;
using DotNetBrightener.Identity.Models.Defaults;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Identity.Services;

/// <summary>
/// Generic implementation of user management operations
/// </summary>
/// <typeparam name="TUser">The type of user entity</typeparam>
public class UserManager<TUser> : IUserManager<TUser> where TUser : User
{
    private readonly IIdentityDbContext _context;
    private readonly ILogger<UserManager<TUser>> _logger;
    private readonly IUserPasswordManager<TUser> _passwordManager;

    public UserManager(
        IIdentityDbContext          context,
        ILogger<UserManager<TUser>> logger,
        IUserPasswordManager<TUser> passwordManager)
    {
        _context = context;
        _logger = logger;
        _passwordManager = passwordManager;
    }

    public async Task<IdentityResult> CreateAsync(TUser user)
    {
        try
        {
            if (user == null)
                return IdentityResult.Failed("User cannot be null");

            // Validate user data
            var validationResult = await ValidateUserAsync(user);
            if (!validationResult.Succeeded)
                return validationResult;

            // Check if username already exists
            if (await UserExistsAsync(user.UserName!))
                return IdentityResult.Failed("Username already exists");

            // Check if email already exists
            if (!string.IsNullOrEmpty(user.Email) && await FindByEmailAsync(user.Email) != null)
                return IdentityResult.Failed("Email already exists");

            // Normalize username and email
            user.NormalizedUserName = user.UserName?.ToUpperInvariant();
            user.NormalizedEmail = user.Email?.ToUpperInvariant();

            // Set audit fields
            user.CreatedDate = DateTimeOffset.UtcNow;
            user.CreatedBy = "System"; // TODO: Get current user context

            _context.Set<TUser>().Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserName} created successfully", user.UserName);
            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {UserName}", user?.UserName);
            return IdentityResult.Failed("An error occurred while creating the user");
        }
    }

    public async Task<IdentityResult> UpdateAsync(TUser user)
    {
        try
        {
            if (user == null)
                return IdentityResult.Failed("User cannot be null");

            var existingUser = await FindByIdAsync(user.Id.ToString());
            if (existingUser == null)
                return IdentityResult.Failed("User not found");

            // Validate user data
            var validationResult = await ValidateUserAsync(user);
            if (!validationResult.Succeeded)
                return validationResult;

            // Update fields
            existingUser.UserName = user.UserName;
            existingUser.NormalizedUserName = user.UserName?.ToUpperInvariant();
            existingUser.Email = user.Email;
            existingUser.NormalizedEmail = user.Email?.ToUpperInvariant();
            existingUser.EmailConfirmed = user.EmailConfirmed;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
            existingUser.MultiFactorAuthEnabled = user.MultiFactorAuthEnabled;
            existingUser.LockoutEnabled = user.LockoutEnabled;
            existingUser.LockoutEnd = user.LockoutEnd;
            existingUser.AccessFailedCount = user.AccessFailedCount;
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.DisplayName = user.DisplayName;
            existingUser.TimeZone = user.TimeZone;
            existingUser.Culture = user.Culture;
            existingUser.IsActive = user.IsActive;

            // Set audit fields
            existingUser.ModifiedDate = DateTimeOffset.UtcNow;
            existingUser.ModifiedBy = "System"; // TODO: Get current user context

            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserName} updated successfully", user.UserName);
            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserName}", user?.UserName);
            return IdentityResult.Failed("An error occurred while updating the user");
        }
    }

    public async Task<IdentityResult> DeleteAsync(TUser user)
    {
        try
        {
            if (user == null)
                return IdentityResult.Failed("User cannot be null");

            var existingUser = await FindByIdAsync(user.Id.ToString());
            if (existingUser == null)
                return IdentityResult.Failed("User not found");

            // Soft delete
            existingUser.IsDeleted = true;
            existingUser.DeletedDate = DateTimeOffset.UtcNow;
            existingUser.DeletedBy = "System"; // TODO: Get current user context
            existingUser.DeletionReason = "User deleted";

            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserName} deleted successfully", user.UserName);
            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserName}", user?.UserName);
            return IdentityResult.Failed("An error occurred while deleting the user");
        }
    }

    public async Task<TUser?> FindByIdAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var id))
            return null;

        return await _context.Set<TUser>()
                           .Where(u => !u.IsDeleted)
                           .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<TUser?> FindByNameAsync(string userName)
    {
        if (string.IsNullOrEmpty(userName))
            return null;

        var normalizedUserName = userName.ToUpperInvariant();
        return await _context.Set<TUser>()
                           .Where(u => !u.IsDeleted)
                           .FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName);
    }

    public async Task<TUser?> FindByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
            return null;

        var normalizedEmail = email.ToUpperInvariant();
        return await _context.Set<TUser>()
                           .Where(u => !u.IsDeleted)
                           .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
    }

    public async Task<IList<TUser>> GetUsersAsync()
    {
        return await _context.Set<TUser>()
                           .Where(u => !u.IsDeleted)
                           .OrderBy(u => u.UserName)
                           .ToListAsync();
    }

    public async Task<IList<TUser>> GetUsersInAccountAsync(Guid accountId)
    {
        return await _context.Set<TUser>()
                           .Where(u => !u.IsDeleted)
                           .Where(u => u.AccountMemberships.Any(am => am.AccountId == accountId && am.IsActive))
                           .OrderBy(u => u.UserName)
                           .ToListAsync();
    }

    public async Task<bool> UserExistsAsync(string userName)
    {
        if (string.IsNullOrEmpty(userName))
            return false;

        var normalizedUserName = userName.ToUpperInvariant();
        return await _context.Set<TUser>()
                           .Where(u => !u.IsDeleted)
                           .AnyAsync(u => u.NormalizedUserName == normalizedUserName);
    }

    public async Task<IdentityResult> SetEmailAsync(TUser user, string email)
    {
        try
        {
            if (user == null)
                return IdentityResult.Failed("User cannot be null");

            user.Email = email;
            user.NormalizedEmail = email?.ToUpperInvariant();
            user.EmailConfirmed = false; // Reset confirmation when email changes

            return await UpdateAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting email for user {UserName}", user?.UserName);
            return IdentityResult.Failed("An error occurred while setting the email");
        }
    }

    public async Task<IdentityResult> SetPhoneNumberAsync(TUser user, string phoneNumber)
    {
        try
        {
            if (user == null)
                return IdentityResult.Failed("User cannot be null");

            user.PhoneNumber = phoneNumber;
            user.PhoneNumberConfirmed = false; // Reset confirmation when phone changes

            return await UpdateAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting phone number for user {UserName}", user?.UserName);
            return IdentityResult.Failed("An error occurred while setting the phone number");
        }
    }

    public async Task<IdentityResult> ConfirmEmailAsync(TUser user, string token)
    {
        try
        {
            if (user == null)
                return IdentityResult.Failed("User cannot be null");

            // TODO: Implement token validation
            // For now, just mark as confirmed
            user.EmailConfirmed = true;

            return await UpdateAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming email for user {UserName}", user?.UserName);
            return IdentityResult.Failed("An error occurred while confirming the email");
        }
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(TUser user)
    {
        // TODO: Implement proper token generation
        // For now, return a simple token
        await Task.CompletedTask;
        return Guid.NewGuid().ToString();
    }

    public async Task<IdentityResult> SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd)
    {
        try
        {
            if (user == null)
                return IdentityResult.Failed("User cannot be null");

            user.LockoutEnd = lockoutEnd;

            return await UpdateAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting lockout end date for user {UserName}", user?.UserName);
            return IdentityResult.Failed("An error occurred while setting the lockout end date");
        }
    }

    public async Task<IdentityResult> AccessFailedAsync(TUser user)
    {
        try
        {
            if (user == null)
                return IdentityResult.Failed("User cannot be null");

            user.AccessFailedCount++;

            // TODO: Implement lockout policy
            // For now, lock out after 5 failed attempts
            if (user.AccessFailedCount >= 5 && user.LockoutEnabled)
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(30);
            }

            return await UpdateAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing access failed count for user {UserName}", user?.UserName);
            return IdentityResult.Failed("An error occurred while processing the access failure");
        }
    }

    public async Task<IdentityResult> ResetAccessFailedCountAsync(TUser user)
    {
        try
        {
            if (user == null)
                return IdentityResult.Failed("User cannot be null");

            user.AccessFailedCount = 0;

            return await UpdateAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting access failed count for user {UserName}", user?.UserName);
            return IdentityResult.Failed("An error occurred while resetting the access failed count");
        }
    }

    public async Task<bool> IsLockedOutAsync(TUser user)
    {
        if (user == null)
            return false;

        return user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;
    }

    public async Task<IdentityResult> SetTwoFactorEnabledAsync(TUser user, bool enabled)
    {
        try
        {
            if (user == null)
                return IdentityResult.Failed("User cannot be null");

            user.MultiFactorAuthEnabled = enabled;

            return await UpdateAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting two-factor enabled for user {UserName}", user?.UserName);
            return IdentityResult.Failed("An error occurred while setting two-factor authentication");
        }
    }

    public async Task<bool> GetTwoFactorEnabledAsync(TUser user)
    {
        if (user == null)
            return false;

        await Task.CompletedTask;
        return user.MultiFactorAuthEnabled;
    }

    private async Task<IdentityResult> ValidateUserAsync(TUser user)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(user.UserName))
            errors.Add("Username is required");

        if (user.UserName?.Length > 256)
            errors.Add("Username cannot exceed 256 characters");

        if (!string.IsNullOrEmpty(user.Email) && user.Email.Length > 256)
            errors.Add("Email cannot exceed 256 characters");

        if (!string.IsNullOrEmpty(user.Email) && !IsValidEmail(user.Email))
            errors.Add("Invalid email format");

        await Task.CompletedTask;

        return errors.Any() ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Non-generic implementation of user management operations using default IdentityUser
/// </summary>
public class UserManager : UserManager<IdentityUser>, IUserManager
{
    public UserManager(
        IIdentityDbContext context,
        ILogger<UserManager> logger,
        IUserPasswordManager passwordManager)
        : base(context, logger, passwordManager)
    {
    }
}
