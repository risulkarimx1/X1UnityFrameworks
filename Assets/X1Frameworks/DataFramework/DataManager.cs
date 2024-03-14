using System.Reflection;
using Debug = UnityEngine.Debug;

namespace X1Frameworks.DataFramework
{
    public class DataManager
    {
        public static string Key = "1234567890abcdef1234567890abcdef";
        public static string IV = "1234567890abcdef";

        private IEncryptionService encryptionService;
        private IDataHandler _dataHandler;

        private PlayerData _playerData;
        public DataManager()
        {
            encryptionService = new AesEncryptionService();
            _dataHandler = new JsonFileDataHandler(encryptionService);
            _playerData = Load<PlayerData>();
            Debug.Log($"Loaded value: {_playerData.Level}");
        }
        
        public void Save<T>(T data) where T : BaseData
        {
            string fileName = GetIdentifier<T>();
            _dataHandler.Save(fileName, data);
        }

        public T Load<T>() where T : BaseData, new()
        {
            string fileName = GetIdentifier<T>();
            return _dataHandler.Load<T>(fileName, Key);
        }
        
        private string GetIdentifier<T>() where T : BaseData
        {
            var id  = typeof(T).GetCustomAttribute<DataIdentifierAttribute>()?.Identifier ?? typeof(T).Name;
            var _dataIdentifier = $"{id}_v{Key}.json";

            return _dataIdentifier;
        }

        public void AddPlayerLevel()
        {
            Debug.Log($"Before: {_playerData.Level}");
            _playerData.Level++;
            Debug.Log($"After: {_playerData.Level}");
        }

        public void SavePlayerData()
        {
            Save(_playerData);
        }
    }
}