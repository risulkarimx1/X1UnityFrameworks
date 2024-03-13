using System;

namespace X1Frameworks.DataFramework
{
    public interface IDataHandler
    {
        void Load(BaseData data, Action onComplete);

        void Save(BaseData data, Action onComplete);
    }
}