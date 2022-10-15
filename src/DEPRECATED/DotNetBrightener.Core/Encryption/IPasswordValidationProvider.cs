using System;

namespace DotNetBrightener.Core.Encryption;

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