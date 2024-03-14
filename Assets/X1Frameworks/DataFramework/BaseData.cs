namespace X1Frameworks.DataFramework
{
    
    [DataIdentifier("player_data")]
    public class PlayerData: BaseData
    {
        private int _level = 0;

        public int Level
        {
            get => _level;
            set
            {
                _level = value;
                SetDirty();
            }
        }
    }
    
    [System.Serializable]
    public abstract class BaseData
    {
        private bool _isDirty = false;
        
        private string _dataIdentifier;
        private string _dataVersion;
        

        public void SetDirty()
        {
            _isDirty = true;
        }
        
        // private string GetIdentifier<T>() where T : BaseData
        // {
        //     if (_dataIdentifier != null && _dataVersion == DataManager.Key) return _dataIdentifier;
        //     
        //     var id  = typeof(T).GetCustomAttribute<DataIdentifierAttribute>()?.Identifier ?? typeof(T).Name;
        //     _dataIdentifier = $"{id}_v{DataManager.Key}.json";
        //     _dataVersion = DataManager.Key;
        //
        //     return _dataIdentifier;
        // }

        // public void Save<T>() where T : BaseData
        // {
        //     if(_isDirty == false) return;
        //     _dataHandler.Save(GetIdentifier<T>(), this);
        //     _isDirty = false;
        // }
        //
        // public T Load<T>() where T : BaseData, new()
        // {
        //     return _dataHandler.Load<T>(GetIdentifier<T>(), DataManager.Key);
        // }
    }
}