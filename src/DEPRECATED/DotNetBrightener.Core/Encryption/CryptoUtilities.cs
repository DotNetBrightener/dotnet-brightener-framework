using System;
using System.Security.Cryptography;
using System.Text;

namespace DotNetBrightener.Core.Encryption;

public class CryptoUtilities
{
    public const int DefaultSaltSize = 16;
        
    /// <summary>
    ///		Generate a random string for password salt, use for creating a key of the password
    /// </summary>
    /// <returns>
    ///		Random string of key used to encrypt the password
    /// </returns>
    public static string CreateRandomToken()
    {
        var provider       = new RNGCryptoServiceProvider();
        var rnd            = new Random();
        var randomSaltSite = rnd.Next(DefaultSaltSize, DefaultSaltSize * 5);
        var bytes          = new byte[randomSaltSite];
        provider.GetBytes(bytes);

        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Generate a random string
    /// </summary>
    /// <returns>A random string</returns>
    public static string GenerateRandomString(int length = 20)
    {
        const string availableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$&^=-";
        var          random         = new Random();

        var builder = new StringBuilder(length);
        for (var i = 0; i < length; i++)
        {
            var @char = availableChars[random.Next(0, availableChars.Length)];
            builder.Append(@char);
        }

        return builder.ToString();
    }
}