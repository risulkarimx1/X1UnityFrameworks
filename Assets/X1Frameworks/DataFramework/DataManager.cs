using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private List<Type> _derivedTypesCache = null;
        private MethodInfo _initializeDataTypeMethodCache = null;

        public DataManager()
        {
            EnsureInitializedAsync().Forget();
        }
        
        private void CacheReflectionResults()
        {
            if (_derivedTypesCache == null)
            {
                var baseDataType = typeof(BaseData);
                _derivedTypesCache = Assembly.GetAssembly(baseDataType)
                    .GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(baseDataType))
                    .ToList();
            }

            if (_initializeDataTypeMethodCache == null)
            {
                _initializeDataTypeMethodCache = typeof(DataManager).GetMethod(nameof(InitializeDataType), BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }
        
        private async UniTask EnsureInitializedAsync()
        {
            if(_isInitialized) return;
            
            CacheReflectionResults();
            
            encryptionService = new AesEncryptionService();
            _dataHandler = new JsonFileDataHandler(encryptionService);
            
            _typeToDataMatch.Clear();
            
            await UniTask.WhenAll(_derivedTypesCache.Select(async type =>
            {
                var genericMethod = _initializeDataTypeMethodCache.MakeGenericMethod(type);
                await (UniTask)genericMethod.Invoke(this, null);
            }));
            
            _isInitialized = true;
        }

        private UniTask<T> LoadAsync<T>() where T : BaseData, new()
        {
            var fileName = GetIdentifier(typeof(T));
            return _dataHandler.LoadAsync<T>(fileName, Key);
        }

        private async UniTask InitializeDataType<T>() where T : BaseData, new()
        {
            var data = await LoadAsync<T>();
            _typeToDataMatch.Add(typeof(T), data);
        }

        public UniTask SaveAsync<T>() where T : BaseData
        {
            BaseData data = _typeToDataMatch[typeof(T)];
            
            var fileName = GetIdentifier(typeof(T));
            return _dataHandler.SaveAsync(fileName, data);
        }
        
        public async UniTask SaveAllAsync()
        {
            await UniTask.WhenAll(_typeToDataMatch.Select(entry =>
            {
                var fileName = GetIdentifier(entry.Key);
                return _dataHandler.SaveAsync(fileName, entry.Value);
            }));
        }
        
        private string GetIdentifier(Type t)
        {
            if (_typeToFileNameMatch.ContainsKey(t)) 
                return _typeToFileNameMatch[t];
            
            var attribute  = t.GetCustomAttribute<DataIdentifierAttribute>()?.Identifier ?? t.Name;
            var dataIdentifier = $"{attribute}_v{Key}.json";
            _typeToFileNameMatch.Add(t, dataIdentifier);

            return _typeToFileNameMatch[t];
        }
        
        public T Get<T>() where T : BaseData
        {
             return _typeToDataMatch[typeof(T)] as T;
        }
    }
}