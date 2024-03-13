using System.Text;

namespace X1Frameworks.DataFramework
{
    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] key;
        private readonly byte[] iv;

        // Constructor where key and IV are initialized
        public EncryptionService(string encryptionKey, string initializationVector)
        {
            key = Encoding.UTF8.GetBytes(encryptionKey);
            iv = Encoding.UTF8.GetBytes(initializationVector);
        }

        public string EncryptString(string plainText)
        {
            // Implement encryption logic using the key and IV
            // Similar to the EncryptString method shown previously
        }

        public string DecryptString(string cipherText)
        {
            // Implement decryption logic using the key and IV
            // Similar to the DecryptString method shown previously
        }
    }
}