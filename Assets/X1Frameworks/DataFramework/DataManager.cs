using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Zenject;

namespace X1Frameworks.DataFramework
{
    public class DataManager : IInitializable
    {
        [Inject] private IDataHandler _dataHandler;
        
        private readonly Dictionary<Type, BaseData> _dataByTypes = new();

        private bool _isInitialized;

        public void Initialize()
        {
            LoadAll();
        }


        private void LoadAll()
        {
            _dataByTypes.Clear();

            var dataTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetInterfaces().Contains(typeof(IData)) && !t.IsAbstract && t.IsClass)
                .Select(Activator.CreateInstance)
                .Cast<IData>();
            
            var loadedCount = 0;
            foreach (var data in dataTypes)
            {
                var baseData = (BaseData)data;
                _dataHandler.Load(baseData, () =>
                {
                    _dataByTypes.Add(baseData.GetType(), baseData);
                    loadedCount++;
                    baseData.OnLoaded();
                });
            }
        }

        [PublicAPI]
        public bool IsInitialized()
        {
            return _isInitialized;
        }

        [PublicAPI]
        public BaseData GetBaseData(Type baseDataType)
        {
            return _dataByTypes.TryGetValue(baseDataType, out var result) ? result : null;
        }

        internal void Save(BaseData data, Action onComplete)
        {
            _dataHandler.Save(data, onComplete);
        }
    }
}