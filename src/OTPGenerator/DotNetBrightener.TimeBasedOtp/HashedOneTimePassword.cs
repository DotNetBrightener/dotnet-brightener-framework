﻿using System.Security.Cryptography;
using System.Text;

namespace DotNetBrightener.TimeBasedOtp;

internal static class HashedOneTimePassword
{
    public static string GeneratePassword(string secret, 
                                          long iterationNumber, 
                                          int digits = 6,
                                          bool ignoreSpaces = false)
    {
        byte[] counter = BitConverter.GetBytes(iterationNumber);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(counter);

        if (ignoreSpaces)
        {
            secret = secret.Replace(" ", "");
        }

        byte[] key = Encoding.ASCII.GetBytes(secret);

        HMACSHA1 hmac = new HMACSHA1(key);

        byte[] hash = hmac.ComputeHash(counter);

        int offset = hash[hash.Length - 1] & 0xf;

        // Convert the 4 bytes into an integer, ignoring the sign.
        int binary =
            ((hash[offset] & 0x7f) << 24)
          | (hash[offset + 1] << 16)
          | (hash[offset + 2] << 8)
          | (hash[offset + 3]);

        // Limit the number of digits
        int password = binary % (int)Math.Pow(10, digits);

        // Pad to required digits
        return password.ToString(new string('0', digits));
    }
}