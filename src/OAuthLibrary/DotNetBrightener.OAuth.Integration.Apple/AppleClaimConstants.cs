namespace DotNetBrightener.OAuth.Integration.Apple;

internal static class AppleClaimConstants
{
    internal const string Issuer     = "iss";
    internal const string IssuedAt   = "iat";
    internal const string Expiration = "exp";
    internal const string Audience   = "aud";
    internal const string Sub        = "sub";
    internal const string KeyID      = "kid";

    internal const string Email              = "email";
    internal const string EmailVerified      = "email_verified";
    internal const string AuthenticationTime = "auth_time";
}

internal static class TokenType
{
    /// <summary>
    /// Used for retrieving a JSON Web Token that contains the user’s identity information.
    /// </summary>
    public const string AuthorizationCode = "authorization_code";
    /// <summary>
    /// Used for verifying if a token is valid.
    /// </summary>
    public const string RefreshToken = "refresh_token";
}