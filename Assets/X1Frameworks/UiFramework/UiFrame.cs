using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Vengadores.UIFramework;
using X1Frameworks.LogFramework;
using Debug = X1Frameworks.LogFramework.Debug;

namespace X1Frameworks.UiFramework
{
    /// <summary>
    /// This is the centralized access point for all things UI.
    /// All your calls should be directed at this.
    /// </summary>
    public class UIFrame : MonoBehaviour
    {
        private Canvas MainCanvas { get; set; }

        private GraphicRaycaster _graphicRaycaster;

        private UiSettings _uiSettings;

        
        private readonly Dictionary<string, UILayer> _layersByName = new();
        private readonly Dictionary<Type, UILayer> _layersByScreenType = new();

        private const int MaxScreenEvents = (int) OnScreenEvent.MAX;
        private readonly Dictionary<Type, Action>[] _onSpecificScreenEvent = new Dictionary<Type, Action>[MaxScreenEvents];
        private readonly Action<UiScreenBase>[] _onScreenEvent = new Action<UiScreenBase>[MaxScreenEvents];

        private int _screenBlockCount;

        private bool _isInitialized;

        internal void Construct(UiSettings uiSettings, Canvas canvas)
        {
            _uiSettings = uiSettings;
            MainCanvas = canvas;
        }

        [PublicAPI] public void Initialize(Camera cam = null)
        {
            for (int i = 0; i < (int)OnScreenEvent.MAX; i++)
            {
                _onSpecificScreenEvent[i] = new Dictionary<Type, Action>();
            }
            
            if (cam != null)
            {
                MainCanvas.worldCamera = cam; 
                MainCanvas.sortingOrder = _uiSettings.orderInLayer;
                MainCanvas.sortingLayerName = _uiSettings.sortingLayerName;
                MainCanvas.sortingLayerID = SortingLayer.NameToID(_uiSettings.sortingLayerName);
            }

            _graphicRaycaster = MainCanvas.GetComponent<GraphicRaycaster>();
            
            foreach (var layerInfo in _uiSettings.layers)
            {
                layerInfo.BackgroundBlockerColor = _uiSettings.backgroundBlockerColor;
                CreateLayer(layerInfo);
            }
            
            foreach (var layer in GetAllLayers())
            {
                foreach (var screenType in layer.GetAllScreenTypes())
                {
                    _layersByScreenType.Add(screenType, layer);
                }
            }

            _isInitialized = true;
        }
        
        private UILayer CreateLayer(LayerInfo layerInfo)
        {
            var layerObject = new GameObject(layerInfo.Name);
            
            var layer = layerObject.AddComponent<UILayer>();
            
            // Add rect transform and set to full stretched
            var layerRT = layerObject.AddComponent<RectTransform>();
            layerRT.anchorMin = new Vector2(0, 0);
            layerRT.anchorMax = new Vector2(1, 1);
            
            // Move layer to UIFrame
            layerObject.transform.SetParent(transform, false);
            
            // Set rect transform size delta to ensure full stretched
            layerRT.sizeDelta = Vector3.zero;
            
            // Add to lookup dict
            _layersByName.Add(layerInfo.Name, layer);

            // Initialize layer, this will create the screens
            layer.InitializeLayer(this, layerInfo);

            // Register screen block requests
            layer.RequestUIInteractionBlock += RequestUIInteractionBlock;
            layer.RequestUIInteractionUnblock += RequestUIInteractionUnblock;
            return layer;
        }

#if UNITY_ANDROID || UNITY_EDITOR
        // Back button listener for android
        private void Update()
        {
            if(!_isInitialized) return;
            
            // KeyCode.Escape is back button on android
            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            
            // if it is not enabled, opening or closing animation is active
            if(!_graphicRaycaster.enabled) return;

            // Get popup layers
            var popupLayers = _layersByName.Values.ToList()
                .Where(l => l.GetLayerInfo().LayerType == LayerType.Popup);

            // Reverse the list to get front layers first
            var sortedPopupLayers = popupLayers.ToList();
            sortedPopupLayers.Reverse();
            
            // Iterate all layers
            foreach (var uiLayer in sortedPopupLayers)
            {
                // Get visible popups
                var visiblePopups = uiLayer.GetVisibleScreens();

                // Discard the layer if no popup visible
                if (visiblePopups.Count <= 0) continue;
                
                // Sort the list by sibling index
                visiblePopups.Sort((a, b) => 
                    b.transform.GetSiblingIndex().CompareTo(a.transform.GetSiblingIndex()));
                        
                // Close the top most popup
                var popupTypeToClose = visiblePopups[0].GetType();
                var screenInfo = uiLayer.GetLayerInfo().Screens.ToList().Find(
                    li => li.Prefab != null && li.Prefab.GetType() == popupTypeToClose);
                
                if (screenInfo.CloseWithEscape)
                {
                    uiLayer.CloseScreen(popupTypeToClose);
                }
                return;
            }
        }
#endif
        [PublicAPI] 
        public UILayer AddLayer(string newLayerName, LayerType newLayerType, string beforeThisLayerName)
        {
            var layerInfo = new LayerInfo
            {
                Name = newLayerName,
                LayerType = newLayerType,
                Screens = new List<ScreenInfo>()
            };
            layerInfo.BackgroundBlockerColor = _uiSettings.backgroundBlockerColor;
            var layerCreated = CreateLayer(layerInfo);

            var siblingLayer = GetLayerByName(beforeThisLayerName);
            if (siblingLayer != null)
            {
                layerCreated.transform.SetSiblingIndex(siblingLayer.transform.GetSiblingIndex());
            }

            return layerCreated;
        }

        [PublicAPI] 
        public void AddScreenInfo(UILayer layer, ScreenInfo screenInfo)
        {
            if (layer != null)
            {
                layer.AddScreenInfo(screenInfo);
                _layersByScreenType.Add(screenInfo.Prefab.GetType(), layer); 
            }
        }

        [PublicAPI] 
        public void RemoveScreenInfo<T>()
        {
            RemoveScreenInfo(typeof(T));
        }
        
        [PublicAPI] 
        public void RemoveScreenInfo(Type screenType)
        {
            var layer = GetLayerByScreenType(screenType);
            if (layer == null)
            {
                Debug.LogError($"Layer not found for the screen type of {screenType.Name}", LogContext.UIFrame);
                return;
            }

            if (layer.RemoveScreenInfo(screenType))
            {
                _layersByScreenType.Remove(screenType);
            }
        }
        
        [PublicAPI] 
        public UILayer GetLayerByName(string layerName)
        {
            return _layersByName.TryGetValue(layerName, out var layer) ? layer : null;
        }
        
        [PublicAPI] public UILayer GetLayerByScreenType<T>()
        {
            return GetLayerByScreenType(typeof(T));
        }
        
        [PublicAPI] 
        public UILayer GetLayerByScreenType(Type screenType)
        {
            return _layersByScreenType.TryGetValue(screenType, out var layer) ? layer : null;
        }
        
        [PublicAPI] 
        public List<UILayer> GetAllLayers()
        {
            return _layersByName.Values.ToList();
        }

        [PublicAPI] 
        public T GetScreen<T>() where T : UiScreenBase
        {
            var layer = GetLayerByScreenType<T>();
            return layer != null ? layer.GetScreen<T>() as T : null;
        }
        
        [PublicAPI] 
        public T Open<T>(IScreenProperties properties = null) where T : UiScreenBase
        {
            return GetLayerByScreenType<T>()?.OpenScreen<T>(properties);
        }
        
        [PublicAPI] 
        public UiScreenBase Open(Type screenType, IScreenProperties properties = null)
        {
            return GetLayerByScreenType(screenType)?.OpenScreen(screenType, properties);
        }
        
        [PublicAPI] 
        public void Close<T>() where T : UiScreenBase
        {
            GetLayerByScreenType<T>()?.CloseScreen<T>();
        }
        
        [PublicAPI] 
        public void Close(Type screenType)
        {
            var layer = GetLayerByScreenType(screenType);
            if (layer != null)
            {
                layer.CloseScreen(screenType);
            }
        }

        [PublicAPI] 
        public bool IsInitialized()
        {
            return _isInitialized;
        }
        
        
        [PublicAPI]  
        public bool IsVisible<T>() where T : UiScreenBase
        {
            var screen = GetScreen<T>();
            return screen != null && screen.GetState() != ScreenState.Closed;
        }
        
        [PublicAPI] public bool IsOpen<T>() where T : UiScreenBase
        {
            var screen = GetScreen<T>();
            return screen != null && screen.GetState() == ScreenState.Opened;
        }

        [PublicAPI] 
        public void RequestUIInteractionBlock()
        {
            _screenBlockCount++;
            if (_graphicRaycaster != null) _graphicRaycaster.enabled = false;
        }

        [PublicAPI] 
        public void RequestUIInteractionUnblock()
        {
            _screenBlockCount--;
            if (_graphicRaycaster == null || _screenBlockCount > 0) return;
            _screenBlockCount = 0;
            _graphicRaycaster.enabled = true;
        }

        [PublicAPI] public bool IsInteractable()
        {
            return _graphicRaycaster.enabled;
        }

        internal void OnScreenEventInternal(OnScreenEvent ev, UiScreenBase screen)
        {
            var actionDict = _onSpecificScreenEvent[(int) ev];
            var t = screen.GetType();
            if (actionDict.ContainsKey(t)) actionDict[t]?.Invoke();
            
            _onScreenEvent[(int) ev]?.Invoke(screen);
        }

        [PublicAPI]
        public void AddEventForAllScreens(OnScreenEvent ev, Action<UiScreenBase> cb)
        {
            _onScreenEvent[(int) ev] += cb;
        }
        
        [PublicAPI]
        public void RemoveEventForAllScreens(OnScreenEvent ev, Action<UiScreenBase> cb)
        {
            _onScreenEvent[(int) ev] -= cb;
        }
        
        [PublicAPI]
        public void AddEventForScreen<T>(OnScreenEvent ev, Action cb)
        {
            var actionDict = _onSpecificScreenEvent[(int) ev];
            var screen = typeof(T);
            if (!actionDict.TryAdd(screen, cb)) actionDict[screen] += cb;
        }
        
        [PublicAPI]
        public void RemoveEventForScreen<T>(OnScreenEvent ev, Action cb)
        {
            var actionDict = _onSpecificScreenEvent[(int) ev];
            var screen = typeof(T);
            if (actionDict.ContainsKey(screen)) actionDict[screen] -= cb;
        }
    }
}