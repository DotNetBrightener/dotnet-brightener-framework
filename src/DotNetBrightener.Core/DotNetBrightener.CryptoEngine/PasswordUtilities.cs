using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DotNetBrightener.CryptoEngine
{
    public static class PasswordUtilities
    {
        public const int DefaultSaltSize = 16;

        public const int DefaultPasswordValidationTokenLength = 5;

        /// <summary>
        /// Generate a random string for password salt, use for creating a key of the password
        /// </summary>
        /// <returns>Random string of password salt</returns>
        public static string CreatePasswordSalt()
        {
            var provider = new RNGCryptoServiceProvider();
            var rnd = new Random();
            var randomSaltSite = rnd.Next(DefaultSaltSize, DefaultSaltSize * 5);
            var bytes = new byte[randomSaltSite];
            provider.GetBytes(bytes);
            var result = string.Concat(Convert.ToBase64String(bytes), GenerateRandomString());
            return result;
        }

        /// <summary>
        /// Generate a random string
        /// </summary>
        /// <returns>A random string</returns>
        public static string GenerateRandomString(int length = 15)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();

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
        public static string GenerateRandomNumericString(int length = 15)
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

        /// <summary>
        /// Create the encrypted password
        /// </summary>
        /// <param name="password">Password to be encrypted</param>
        /// <param name="passwordSalt">Password salt for encrypted</param>
        /// <returns>An encrypted password</returns>
        public static string CreatePasswordHash(string password, string passwordSalt)
        {
            var  passwordAndSalt = string.Concat(password, passwordSalt);
            var  bytes           = Encoding.Unicode.GetBytes(passwordAndSalt);
            SHA1 provider        = new SHA1Managed();
            bytes = provider.ComputeHash(bytes);
            var result = Convert.ToBase64String(bytes);

            return result;
        }

        public static string CreateRandomToken()
        {
            var dataToHash      = GenerateRandomString(10);
            var dataToHashBytes = Encoding.ASCII.GetBytes(dataToHash);

            using (var md5 = MD5.Create())
            {
                var hashed = md5.ComputeHash(dataToHashBytes);

                return BitConverter.ToString(hashed).Replace("-", "");
            }
        }
    }
}
