using System;
using System.Security.Cryptography;
using System.Text;

namespace DotNetBrightener.Core.Encryption
{
    public static class SymmetricCryptoEngine
    {
        public static string Encrypt(string textToEncrypt, string encryptionKey = "")
        {
            var toEncryptArray = Encoding.UTF8.GetBytes(textToEncrypt);

            var hashmd5 = new MD5CryptoServiceProvider();
            var keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));
            hashmd5.Clear();

            var tdes = new TripleDESCryptoServiceProvider
            {
                Key = keyArray,              //set the secret key for the tripleDES algorithm
                Mode = CipherMode.ECB,       //mode of operation. there are other 4 modes. We choose ECB(Electronic code Book)
                Padding = PaddingMode.PKCS7  //padding mode(if any extra byte added)
            };

            var cTransform = tdes.CreateEncryptor();
            //transform the specified region of bytes array to resultArray
            var resultArray = cTransform.TransformFinalBlock
                    (toEncryptArray, 0, toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor
            tdes.Clear();

            //Return the encrypted data into unreadable string format
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string Decrypt(string textToDecrypt, string encryptionKey = "")
        {
            var toEncryptArray = Convert.FromBase64String(textToDecrypt);

            //if hashing was used get the hash code with regards to your key
            var hashmd5 = new MD5CryptoServiceProvider();
            var keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));
            hashmd5.Clear();

            var tdes = new TripleDESCryptoServiceProvider
            {
                Key = keyArray,               //set the secret key for the tripleDES algorithm
                Mode = CipherMode.ECB,        //mode of operation. there are other 4 modes. We choose ECB(Electronic code Book)
                Padding = PaddingMode.PKCS7   //padding mode(if any extra byte added)
            };

            var cTransform = tdes.CreateDecryptor();
            var resultArray = cTransform.TransformFinalBlock
                    (toEncryptArray, 0, toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor
            tdes.Clear();
            //return the Clear decrypted TEXT
            return Encoding.UTF8.GetString(resultArray);
        }

        public static bool TryDecrypt(string textToDecrypt, out string output, string encryptionKey = "")
        {
			try
			{
				var toEncryptArray = Convert.FromBase64String(textToDecrypt);

				//if hashing was used get the hash code with regards to your key
				var hashmd5 = new MD5CryptoServiceProvider();
				var keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));
				hashmd5.Clear();

				var tdes = new TripleDESCryptoServiceProvider
				{
					Key = keyArray,               //set the secret key for the tripleDES algorithm
					Mode = CipherMode.ECB,        //mode of operation. there are other 4 modes. We choose ECB(Electronic code Book)
					Padding = PaddingMode.PKCS7   //padding mode(if any extra byte added)
				};

				var cTransform = tdes.CreateDecryptor();
				var resultArray = cTransform.TransformFinalBlock
						(toEncryptArray, 0, toEncryptArray.Length);
				//Release resources held by TripleDes Encryptor
				tdes.Clear();
				//return the Clear decrypted TEXT
				output = Encoding.UTF8.GetString(resultArray);

				return true;
			}
			catch (Exception)
			{
				output = string.Empty;
				return false;
			}
        }
    }
}
