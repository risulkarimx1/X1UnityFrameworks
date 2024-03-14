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
    
    [DataIdentifier("currency_data")]
    public class CurrencyData: BaseData
    {
        private int _gold = 0;
        private int _silver = 0;
        
        public int Gold
        {
            get => _gold;
            set => _gold = value;
        }

        public int Silver
        {
            get => _silver;
            set => _silver = value;
        }
    }
    
    [System.Serializable]
    public abstract class BaseData
    {
        public bool IsDirty { get; private set; }
        
        private string _dataIdentifier;
        private string _dataVersion;
        

        public void SetDirty()
        {
            IsDirty = true;
        }
    }
}