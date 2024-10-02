using Microsoft.Extensions.Logging;

namespace DotNetBrightener.CryptoEngine;

/// <summary>
///     Represents the service for generating and validating password
/// </summary>
public interface IPasswordValidationProvider
{
    /// <summary>
    ///     Generates an encrypted password and a key associated with it
    /// </summary>
    /// <param name="plainTextPassword">The password in plain text</param>
    /// <returns>
    ///     A <see cref="Tuple{T1, T2}"/> represents a pair of a salt and the encrypted password
    /// </returns>
    Tuple<string, string> GenerateEncryptedPassword(string plainTextPassword);

    /// <summary>
    ///     Validates the plain text password, provided the encryption key and the encrypted password
    /// </summary>
    /// <param name="plainTextPassword">The password in plain text</param>
    /// <param name="passwordEncryptionKey">The password salt</param>
    /// <param name="hashedPassword">The encrypted password</param>
    /// <returns><c>true</c> if the password is valid and matches the encrypted one, otherwise, <c>false</c></returns>
    bool ValidatePassword(string plainTextPassword, string passwordEncryptionKey, string hashedPassword);
}

public class DefaultPasswordValidationProvider(ICryptoEngine cryptoEngine, 
                                               ILoggerFactory loggerFactory) : IPasswordValidationProvider
{
    private ILogger Logger => loggerFactory.CreateLogger(GetType());

    public virtual Tuple<string, string> GenerateEncryptedPassword(string plainTextPassword)
    {
        // create a key (salt) for hashing the password
        var passwordSalt = CryptoUtilities.CreateRandomToken(24);

        // hash the password with the salt
        var hashedPassword = AesCryptoEngine.Encrypt(plainTextPassword, passwordSalt);

        // encrypt the salt
        var encryptedPasswordSalt = cryptoEngine.EncryptText(passwordSalt);

        return new Tuple<string, string>(encryptedPasswordSalt, hashedPassword);
    }

    public virtual bool ValidatePassword(string plainTextPassword, string passwordEncryptionKey, string hashedPassword)
    {
        // decrypt the key to get the salt
        var passwordSalt = cryptoEngine.DecryptText(passwordEncryptionKey);

        // use the salt to create the hash from plain text password
        try
        {
            if (string.Equals(AesCryptoEngine.Encrypt(plainTextPassword, passwordSalt),
                              hashedPassword,
                              StringComparison.Ordinal))
                return true;
        }
        catch (Exception ex)
        {
            // don't need to handle, try the next one as fall back
            Logger.LogInformation("Error while trying to compare using new encryption algorithm. Switching to old algorithm.");
        }

        // fall back to the old implementation
        if (string.Equals(TripleDesCryptoEngine.Encrypt(plainTextPassword, passwordSalt),
                          hashedPassword,
                          StringComparison.Ordinal))
            return true;

        return false;
    }
}