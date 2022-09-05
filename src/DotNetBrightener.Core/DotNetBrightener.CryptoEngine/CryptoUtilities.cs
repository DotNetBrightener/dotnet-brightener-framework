﻿using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DotNetBrightener.CryptoEngine;

public class CryptoUtilities
{
    public const int DefaultSaltSize = 16;

    /// <summary>
    ///		Generate a random string for password salt, use for creating a key of the password
    /// </summary>
    /// <param name="saltSize">
    ///     Size of the salt to create the complexity of the key. 
    ///     Specify value of <b><c>0</c> (zero)</b> to let the system randomly choose the size
    /// </param>
    /// <returns>
    ///		Random string of key used to encrypt the password
    /// </returns>
    public static string CreateRandomToken(int saltSize = 64)
    {
        var provider = new RNGCryptoServiceProvider();
        if (saltSize == 0)
        {
            var rnd = new Random();
            saltSize = rnd.Next(DefaultSaltSize, DefaultSaltSize * 5);
        }

        var bytes = new byte[saltSize];
        provider.GetBytes(bytes);

        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    ///     Generate a random string
    /// </summary>
    /// <returns>A random string</returns>
    public static string GenerateRandomString(int length = 32)
    {
        const string availableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$&";
        var          random         = new Random();

        var builder = new StringBuilder(length);
        for (var i = 0; i < length; i++)
        {
            var @char = availableChars[random.Next(0, availableChars.Length)];
            builder.Append(@char);
        }

        return builder.ToString();
    }

    public static string GenerateTimeBasedToken(TimeSpan expiresIn, ref string token)
    {
        token ??= CreateRandomToken();

        byte[] time = BitConverter.GetBytes(DateTime.UtcNow.Add(expiresIn).ToBinary());
        byte[] key  = Encoding.UTF8.GetBytes(token);

        return Convert.ToBase64String(time.Concat(key).ToArray());
    }

    public static bool ValidateTimeBasedToken(string token, out string tokenData)
    {
        byte[]   data    = Convert.FromBase64String(token);
        DateTime expires = DateTime.FromBinary(BitConverter.ToInt64(data, 0));

        if (expires < DateTime.UtcNow)
        {
            tokenData = null;
            return false;
        }

        tokenData = Encoding.UTF8.GetString(data.Skip(8).ToArray());

        return true;
    }
}