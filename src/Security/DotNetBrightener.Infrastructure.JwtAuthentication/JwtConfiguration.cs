using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace DotNetBrightener.Infrastructure.JwtAuthentication;

/// <summary>
///     Represents the configuration for JWT
/// </summary>
public class JwtConfiguration
{
    /// <summary>
    ///     Specifies the default expiration of the token
    /// </summary>
    public const int DefaultExpiration = 5;

    /// <summary>
    ///     The name of the service that issues the token
    /// </summary>
    public string Issuer { get; set; }

    /// <summary>
    ///     The name or list of names of the audiences that the token is provided to
    /// </summary>
    public string Audience { get; set; }

    /// <summary>
    ///     The public key for verifying the signature of the generated token
    /// </summary>
    public string SignatureVerificationKey { get; set; }

    /// <summary>
    ///     The private key for verifying the signature of the generated token
    /// </summary>
    public string PrivateSigningKey { get; set; }

    /// <summary>
    ///     Indicates whether the RSA algorithm is used to sign the token 
    /// </summary>
    public bool UseRSASigningVerification => !string.IsNullOrEmpty(PrivateSigningKey);

    /// <summary>
    ///     Indicates the expiration in minutes of the token
    /// </summary>
    public int ExpireAfterMinutes { get; set; }

    [JsonIgnore]
    public string KID { get; set; } = DefaultJwtKId;

    public const string DefaultJwtKId = "_DEFAULT_KID_VALUE";
}