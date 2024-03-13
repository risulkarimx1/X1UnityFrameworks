using System;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace X1Frameworks.DataFramework
{
    public interface IData { }
    
    [Serializable]
    public abstract class BaseData : IData
    {
        [Inject] private DataManager _dataManager;
        public event Action OnMergedCallback;
        public int ModelVersion;
        
        
        [PublicAPI] 
        public void Save(Action onComplete = null)
        {
            OnBeforeSave();
            
            _dataManager.Save(this, () =>
            {
                OnSaved();
                onComplete?.Invoke();
            });
        }
        
        public void MergeWithOther(BaseData other)
        {
            Merge(other);
            OnMergedCallback?.Invoke();
        }
        
        protected abstract void Merge(BaseData other);
        
        public virtual void OnLoaded() { }
        
        public virtual void OnBeforeSave() { }

        public virtual void OnSaved() { }
    }
}
