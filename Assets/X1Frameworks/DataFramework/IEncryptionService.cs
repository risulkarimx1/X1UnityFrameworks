using Cysharp.Threading.Tasks;
using UnityEngine;

namespace X1Frameworks.DataFramework
{
    public interface IEncryptionService
    {
        UniTask<string> EncryptStringAsync(string plainText);
        UniTask<string> DecryptString(string cipherText);
    }
}
