using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace X1Frameworks.DataFramework
{
    public class DataManager
    {
        public static string Key = "1234567890abcdef1234567890abcdef";
        public static string IV = "1234567890abcdef";

        private  IEncryptionService encryptionService;
        private  IDataHandler _dataHandler;
        
        private bool _isInitialized = false;

        private Dictionary<Type, BaseData> _typeToDataMatch = new();
        private Dictionary<Type, string> _typeToFileNameMatch = new();

        public DataManager()
        {
            EnsureInitializedAsync().Forget();
        }
        
        private async UniTask EnsureInitializedAsync()
        {
            if(_isInitialized) return;
            
            encryptionService = new AesEncryptionService();
            _dataHandler = new JsonFileDataHandler(encryptionService);
            
            _typeToDataMatch.Clear();
            
            var baseDataType = typeof(BaseData);
            var derivedTypes = Assembly.GetAssembly(baseDataType)
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(baseDataType));
            
            var method = typeof(DataManager).GetMethod(nameof(InitializeDataType), BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var type in derivedTypes)
            {
                var generic = method.MakeGenericMethod(type);
                await (UniTask)generic.Invoke(this, null);
            }
            
            _isInitialized = true;
        }

        private UniTask<T> LoadAsync<T>() where T : BaseData, new()
        {
            var fileName = GetIdentifier<T>();
            return _dataHandler.LoadAsync<T>(fileName, Key);
        }

        private async UniTask InitializeDataType<T>() where T : BaseData, new()
        {
            var data = await LoadAsync<T>();
            _typeToDataMatch.Add(typeof(T), data);
        }

        public UniTask SaveAsync<T>() where T : BaseData
        {
            var data = _typeToDataMatch[typeof(T)];
            var fileName = GetIdentifier<T>();
            return _dataHandler.SaveAsync(fileName, data);
        }
        
        private string GetIdentifier<T>() where T : BaseData
        {
            if (_typeToFileNameMatch.ContainsKey(typeof(T))) 
                return _typeToFileNameMatch[typeof(T)];
            
            var attribute  = typeof(T).GetCustomAttribute<DataIdentifierAttribute>()?.Identifier ?? typeof(T).Name;
            var dataIdentifier = $"{attribute}_v{Key}.json";
            _typeToFileNameMatch.Add(typeof(T), dataIdentifier);

            return _typeToFileNameMatch[typeof(T)];
        }
        
        public T Get<T>() where T : BaseData
        {
             return _typeToDataMatch[typeof(T)] as T;
        }
    }
}