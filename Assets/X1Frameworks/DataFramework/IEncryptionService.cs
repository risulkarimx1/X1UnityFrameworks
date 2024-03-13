using UnityEngine;

namespace X1Frameworks.DataFramework
{
    public interface IEncryptionService
    {
        string EncryptString(string plainText);
        string DecryptString(string cipherText);
    }
}
