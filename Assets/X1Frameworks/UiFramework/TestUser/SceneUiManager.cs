using UnityEngine;
using UnityEngine.UI;

namespace X1Frameworks.UiFramework.TestUser
{
    public class SceneUiManager: MonoBehaviour
    {
        [SerializeField] private string panelName;
        [SerializeField] private UiScreenBase TestPanelPrefab;

        public async void OpenTestPanel()
        {
            var testPanel = await AddressableLoader.LoadAndInstantiateAsync<UiScreenBase>(panelName);
             var parent = GetComponent<Canvas>().transform;
             testPanel.transform.parent = parent;
            testPanel.Open(new TestPanelProps());
            
        }
    }
    
}