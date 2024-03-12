using UnityEngine;
using X1Frameworks.UiFramework;
using X1Frameworks.UiFramework.Example;
using Zenject;

namespace Project.Installer
{
    [CreateAssetMenu(fileName = "BootSceneInstaller", menuName = "Installers/BooSceneInstaller")]
    public class BootSceneInstaller : ScriptableObjectInstaller<BootSceneInstaller>
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