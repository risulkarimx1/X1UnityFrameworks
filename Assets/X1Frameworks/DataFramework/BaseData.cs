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
    }
}