using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace X1Frameworks.UiFramework.TestUser
{
    public static class AddressableLoader
    {
        public static async UniTask<T> LoadAndInstantiateAsync<T>(string addressableKey, Transform parent = null) where T : UnityEngine.Object
        {
            // First, load the GameObject itself
            var operation = Addressables.LoadAssetAsync<GameObject>(addressableKey);
            await operation.ToUniTask();
    
            if (operation.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                GameObject loadedPrefab = operation.Result;
                // Now, try to get the requested component from the loaded GameObject
                T component = loadedPrefab.GetComponent<T>();
                if (component != null)
                {
                    // Instantiate the GameObject and return the component
                    GameObject instantiatedPrefab = GameObject.Instantiate(loadedPrefab, parent);
                    return instantiatedPrefab.GetComponent<T>();
                }
                else
                {
                    Debug.LogError($"The requested component of type {typeof(T).Name} was not found on the prefab.");
                    return null;
                }
            }
            else
            {
                Debug.LogError($"Failed to load the GameObject with key {addressableKey}.");
                return null;
            }
        }

        public static async UniTaskVoid UnloadAsync(GameObject uiGameObject)
        {
            await UniTask.SwitchToMainThread(); // Ensure we are on the main thread when dealing with Unity objects
            Addressables.ReleaseInstance(uiGameObject);
        }
    }
}
