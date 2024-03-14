using Cysharp.Threading.Tasks;

namespace X1Frameworks.DataFramework
{
    public interface IDataHandler
    {
        UniTask<T> LoadAsync<T>(string fileName, string expectedKeyVersion) where T : BaseData, new();
        UniTask SaveAsync(string dataIdentifier, BaseData baseData);
    }

}