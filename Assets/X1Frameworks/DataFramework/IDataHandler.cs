namespace X1Frameworks.DataFramework
{
    public interface IDataHandler
    {
        T Load<T>(string fileName, string expectedKeyVersion) where T : BaseData, new();
        void Save(string dataIdentifier, BaseData baseData);
    }

}