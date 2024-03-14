using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace X1Frameworks.DataFramework
{
    public class AesEncryptionService : IEncryptionService
    {
        public string EncryptString(string plainText)
        {
            using var aesAlg = Aes.Create();
            aesAlg.Key = Encoding.UTF8.GetBytes(DataManager.Key);
            aesAlg.IV = Encoding.UTF8.GetBytes(DataManager.IV);

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }
            }

            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        public string DecryptString(string cipherText)
        {
            var cipherBytes = Convert.FromBase64String(cipherText);

            using var aesAlg = Aes.Create();
            aesAlg.Key = Encoding.UTF8.GetBytes(DataManager.Key);
            aesAlg.IV = Encoding.UTF8.GetBytes(DataManager.IV);

            var decrypted = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using var msDecrypt = new MemoryStream(cipherBytes);
            using var csDecrypt = new CryptoStream(msDecrypt, decrypted, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            return srDecrypt.ReadToEnd();
        }
    }
}