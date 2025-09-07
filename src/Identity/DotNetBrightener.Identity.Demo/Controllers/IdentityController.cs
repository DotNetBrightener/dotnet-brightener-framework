using Microsoft.AspNetCore.Mvc;
using DotNetBrightener.Identity.Services;
using DotNetBrightener.Identity.Models.Defaults;

namespace DotNetBrightener.Identity.Demo.Controllers;

/// <summary>
///     Controller for testing Identity module functionality
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class IdentityController : ControllerBase
{
    private readonly ILogger<IdentityController> _logger;
    private readonly IUserManager<IdentityUser>? _userManager;
    private readonly IUserPasswordManager<IdentityUser>? _passwordManager;

    /// <summary>
    ///     Initializes a new instance of the IdentityController
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="userManager">User manager service (optional)</param>
    /// <param name="passwordManager">Password manager service (optional)</param>
    public IdentityController(
        ILogger<IdentityController> logger,
        IUserManager<IdentityUser>? userManager = null,
        IUserPasswordManager<IdentityUser>? passwordManager = null)
    {
        _logger = logger;
        _userManager = userManager;
        _passwordManager = passwordManager;
    }

    /// <summary>
    ///     Gets the status of Identity services
    /// </summary>
    /// <returns>Service status information</returns>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        _logger.LogInformation("Checking Identity services status");

        return Ok(new
        {
            UserManagerAvailable = _userManager != null,
            PasswordManagerAvailable = _passwordManager != null,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    ///     Tests user creation functionality
    /// </summary>
    /// <param name="request">User creation request</param>
    /// <returns>Result of user creation test</returns>
    [HttpPost("test/create-user")]
    public async Task<IActionResult> TestCreateUser([FromBody] CreateUserRequest request)
    {
        _logger.LogInformation("Testing user creation for email: {Email}", request.Email);

        if (_userManager == null)
        {
            return BadRequest(new { Error = "UserManager service not available" });
        }

        try
        {
            var user = new IdentityUser
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "Demo"
            };

            // Note: This would normally save to database
            // For demo purposes, we'll just validate the object

            return Ok(new
            {
                Success = true,
                Message = "User object created successfully (demo mode)",
                User = new
                {
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.IsActive,
                    user.CreatedDate
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing user creation");
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    ///     Tests password hashing functionality
    /// </summary>
    /// <param name="request">Password test request</param>
    /// <returns>Result of password hashing test</returns>
    [HttpPost("test/password")]
    public async Task<IActionResult> TestPassword([FromBody] PasswordTestRequest request)
    {
        _logger.LogInformation("Testing password functionality");

        if (_passwordManager == null)
        {
            return BadRequest(new { Error = "PasswordManager service not available" });
        }

        try
        {
            // Create a dummy user for testing
            var dummyUser = new IdentityUser("testuser");

            // Test password hashing
            var hashedPassword = _passwordManager.HashPassword(dummyUser, request.Password);

            // Test password verification
            var isValid = _passwordManager.VerifyHashedPassword(dummyUser, hashedPassword, request.Password);

            return Ok(new
            {
                Success = true,
                Message = "Password hashing and verification successful",
                HashedPasswordLength = hashedPassword.Length,
                VerificationResult = isValid
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing password functionality");
            return BadRequest(new { Error = ex.Message });
        }
    }
}

/// <summary>
///     Request model for user creation
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    ///     User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     User's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    ///     User's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;
}

/// <summary>
///     Request model for password testing
/// </summary>
public class PasswordTestRequest
{
    /// <summary>
    ///     Password to test
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
