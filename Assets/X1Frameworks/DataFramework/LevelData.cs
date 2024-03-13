using UnityEngine;

namespace X1Frameworks.DataFramework
{
    public class LevelData : BaseData
    {
        public int Level = 1;

        protected override void Merge(BaseData other)
        {
            Level = Mathf.Max(Level, ((LevelData)other).Level);
        }
    }
}