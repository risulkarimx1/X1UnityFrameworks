using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace X1Frameworks.UiFramework.Example
{
    public class TestPanelProps : IScreenProperties
    {
    
    }
    public class TestPanel : UIScreen<TestPanelProps>
    {
        [SerializeField] private Button closeButton;

        protected override void OnCreated()
        {
            closeButton.OnClickAsObservable().Subscribe(_ => ForceClose()).AddTo(this);
        }
    }
}