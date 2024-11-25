using System.Security.Cryptography;
using System.Text;

namespace DotNetBrightener.CryptoEngine;

public static class PasswordUtilities
{
    public const int DefaultSaltSize = 16;

    public const int DefaultPasswordValidationTokenLength = 5;

    /// <summary>
    /// Generate a random string for password salt, use for creating a key of the password
    /// </summary>
    /// <returns>Random string of password salt</returns>
    public static string CreatePasswordSalt(int? saltSize = null)
    {
        var rnd            = new Random();
        var randomSaltSite = saltSize ?? rnd.Next(DefaultSaltSize, DefaultSaltSize * 5);
        var bytes          = RandomNumberGenerator.GetBytes(randomSaltSite);
        var result         = string.Concat(Convert.ToBase64String(bytes), GenerateRandomString());

        return result;
    }

    /// <summary>
    /// Generate a random string
    /// </summary>
    /// <returns>A random string</returns>
    public static string GenerateRandomString(int length = 16, bool includeSymbols = false)
    {
        string chars  = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        if (includeSymbols)
        {
            chars += "!@#$&\\*()-+'\",.[]|";
        }

        var          random = new Random();

        var result = new string(
                                Enumerable.Repeat(chars, length)
                                          .Select(s => s[random.Next(s.Length)])
                                          .ToArray());

        return result;
    }

    /// <summary>
    ///     Generate a random numeric string
    /// </summary>
    /// <returns>A random string with only digits</returns>
    public static string GenerateRandomNumericString(int length = 16)
    {
        const string chars  = "0123456789";
        var          random = new Random();

        var result = new string(
                                Enumerable.Repeat(chars, length)
                                          .Select(s => s[random.Next(s.Length)])
                                          .ToArray()
                               );

        return result;
    }

    public static string JsBtoAString(string input)
    {
        var bytes    = Encoding.GetEncoding(28591).GetBytes(input);
        var toReturn = Convert.ToBase64String(bytes);

        return toReturn;
    }

    public static string JsAtoBString(string input)
    {
        var bytes    = Convert.FromBase64String(input);
        var toReturn = Encoding.GetEncoding(28591).GetString(bytes);

        return toReturn;
    }
}