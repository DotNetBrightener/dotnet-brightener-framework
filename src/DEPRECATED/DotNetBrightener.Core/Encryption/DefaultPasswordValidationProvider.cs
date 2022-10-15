using System;

namespace DotNetBrightener.Core.Encryption;

public class DefaultPasswordValidationProvider : IPasswordValidationProvider
{
    private readonly ICryptoEngine _cryptoEngine;

    public DefaultPasswordValidationProvider(ICryptoEngine cryptoEngine)
    {
        _cryptoEngine = cryptoEngine;
    }

    public Tuple<string, string> GenerateEncryptedPassword(string plainTextPassword)
    {
        // create a key (salt) for hashing the password
        var passwordSalt = CryptoUtilities.CreateRandomToken();

        // hash the password with the salt
        var hashedPassword = SymmetricCryptoEngine.Encrypt(plainTextPassword, passwordSalt);

        // encrypt the salt
        var encryptedPasswordSalt = _cryptoEngine.EncryptText(passwordSalt);

        return new Tuple<string, string>(encryptedPasswordSalt, hashedPassword);
    }

    public bool ValidatePassword(string plainTextPassword, string passwordEncryptionKey, string hashedPassword)
    {
        // decrypt the key to get the salt
        var passwordSalt = _cryptoEngine.DecryptText(passwordEncryptionKey);

        // use the salt to create the hash from plain text password
        var encryptedPassword = SymmetricCryptoEngine.Encrypt(plainTextPassword, passwordSalt);

        return string.Equals(encryptedPassword, hashedPassword, StringComparison.Ordinal);
    }
}