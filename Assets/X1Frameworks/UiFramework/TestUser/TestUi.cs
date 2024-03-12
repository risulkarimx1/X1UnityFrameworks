using System;
using UnityEngine;
using Zenject;

namespace X1Frameworks.UiFramework.TestUser
{
    public class TestUi : MonoBehaviour
    {
        [Inject] private UIFrame _uiFrame;

        private void Start()
        {
            _uiFrame.Initialize(Camera.main);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                var testPanel = _uiFrame.Open<TestPanel>(new TestPanelProps());
            }
        }
    }
}
