using UnityEngine;
using X1Frameworks.UiFramework;
using X1Frameworks.UiFramework.TestUser;
using Zenject;

namespace Project.Installer
{
    [CreateAssetMenu(fileName = "BooSceneInstaller", menuName = "Installers/BooSceneInstaller")]
    public class BooSceneInstaller : ScriptableObjectInstaller<BooSceneInstaller>
    {
        [SerializeField] private TestUi _testUi;
        [SerializeField] private UiSettings uiSettings;
        public override void InstallBindings()
        {
            UIFrame uiFrame = uiSettings.BuildUIFrame();
            Container.Bind<UIFrame>().FromInstance(uiFrame).AsSingle().NonLazy();
            Container.Bind<TestUi>().FromComponentInNewPrefab(_testUi).AsSingle().NonLazy();

        }
    }
}