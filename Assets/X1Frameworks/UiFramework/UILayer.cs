using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using X1Frameworks.LogFramework;
using X1Frameworks.UiFramework;
using Debug = X1Frameworks.LogFramework.Debug;

namespace Vengadores.UIFramework
{
    /// <summary>
    /// UILayer is responsible for opening/closing screens
    /// If layerType is popup, it will add a dark background and control it.
    /// When layerType is popup, screens are set as last sibling when they are opening.
    /// </summary>
    public class UILayer : MonoBehaviour
    { 
        public event Action RequestUIInteractionBlock;
        public event Action RequestUIInteractionUnblock;

        private readonly Dictionary<Type, UiScreenBase> _screensByType = new();
        private readonly Dictionary<Type, ScreenInfo> _screenInfosByType = new();
        
        private PopupBackgroundBlocker _backgroundBlocker;
        
        private LayerInfo _layerInfo;
        private UIFrame _uiFrame;

        internal void InitializeLayer(UIFrame parent, LayerInfo layerInfo)
        {
            _uiFrame = parent;
            _layerInfo = layerInfo;
            
            foreach (var screenInfo in layerInfo.Screens)
            {
                AddScreenInfo(screenInfo);
            }
            
            if (_layerInfo.LayerType == LayerType.Popup)
            {
                CreateBackgroundBlocker();
            }
        }

        internal void AddScreenInfo(ScreenInfo screenInfo)
        {
            if (screenInfo.Prefab == null)
            {
                Debug.LogError($"UIFrame Layer {_layerInfo.Name} has a null reference.", LogContext.UIFrame);
                return;
            }
            
            var screen = screenInfo.Prefab.GetComponent<UiScreenBase>();
            if (screen == null)
            {
                Debug.LogError($"{nameof(UiScreenBase)} component not found on {screenInfo.Prefab.name} in layer {_layerInfo.Name}.", LogContext.UIFrame);
                return;
            }
            
            var screenType = screen.GetType();
            
            if (_screensByType.ContainsKey(screenType) || _screenInfosByType.ContainsKey(screenType))
            {
                Debug.LogError("Screen is already added to the layer!", LogContext.UIFrame);
                return;
            }
            
            _screensByType.Add(screenType, null);
            _screenInfosByType.Add(screenType, screenInfo);

            if (!screenInfo.LoadOnDemand)
            {
                CreateScreen(screenInfo.Prefab);
            }
        }

        internal bool RemoveScreenInfo(Type screenType)
        {
            var screen = GetScreen(screenType);
            if (screen != null)
            {
                var screenState = screen.GetState();
                if (screenState != ScreenState.Closed)
                {
                    Debug.LogError($"Screen should be in closed state.", LogContext.UIFrame);
                    return false;
                }

                Destroy(screen.gameObject);
            }
            
            _screensByType.Remove(screenType);
            _screenInfosByType.Remove(screenType);
            return true;
        }

        private UiScreenBase CreateScreen(Type screenType)
        {
            var screenInfo = GetScreenInfo(screenType);
            return CreateScreen(screenInfo.Prefab);
        }

        private UiScreenBase CreateScreen(Component prefab)
        {
            var screenPrefabComponent = prefab.GetComponent<UiScreenBase>();
            var screenType = screenPrefabComponent.GetType();
            
            var screenObject = Instantiate(prefab.gameObject, transform);
            var screen = screenObject.GetComponent<UiScreenBase>();
            
            if (screenObject.activeSelf) screenObject.SetActive(false);
            
            _screensByType[screenType] = screen;
            
            screen.CloseRequest += OnCloseRequestedByScreen;
            
            screen.OnScreenEvent += _uiFrame.OnScreenEventInternal;
            
            _uiFrame.OnScreenEventInternal(OnScreenEvent.Created,screen);

            screen.InitScreen();

            return screen;
        }
        
        private void CreateBackgroundBlocker()
        {
            var backgroundBlockerObject = new GameObject("BackgroundBlocker");
            _backgroundBlocker = backgroundBlockerObject.AddComponent<PopupBackgroundBlocker>();
            _backgroundBlocker.Init(transform, _layerInfo.BackgroundBlockerColor);
            
            _backgroundBlocker.OnBackgroundBlockerClick += OnBackgroundBlockerClick;
            
            backgroundBlockerObject.SetActive(false);
        }
        
        private void OnBackgroundBlockerClick()
        {
            var visibleScreens = GetVisibleScreens();
            if (visibleScreens.Count == 0) return;
            
            var foremostScreen = visibleScreens.Find(s => s.transform.GetSiblingIndex() == transform.childCount - 1);
            
            var screenInfo = GetScreenInfo(foremostScreen.GetType());
            if (screenInfo.CloseWithBgClick)
            {
                CloseScreen(foremostScreen.GetType());
            }
        }
        
        private void OnCloseRequestedByScreen(Type screenType)
        {
            CloseScreen(screenType);
        }
        
        internal T OpenScreen<T>(IScreenProperties properties = null) where T : UiScreenBase
        {
            return OpenScreen(typeof(T), properties) as T;
        }
        
        internal UiScreenBase OpenScreen(Type screenType, IScreenProperties properties = null)
        {
            var screenInfo = GetScreenInfo(screenType);
            if (screenInfo == null)
            {
                Debug.LogError($"Screen info not found: {screenType}", LogContext.UIFrame);
                return null;
            }
            
            var screen = GetScreen(screenType);
            if (screen == null)
            {
                // Screen is not instantiated, instantiate now
                screen = CreateScreen(screenType);
            }

            if (screen.GetState() != ScreenState.Closed)
            {
                Debug.LogError($"Screen is not in 'Closed' state, can not open: {screenType}", LogContext.UIFrame);
                return screen;
            }
            
            // Request ui interaction block before opening the screen
            RequestUIInteractionBlock?.Invoke();
            
            if (_layerInfo.LayerType == LayerType.Popup)
            {
                screen.Open(properties, () =>
                {
                    // Unblock ui interactions
                    RequestUIInteractionUnblock?.Invoke();
                });
                
                // Show background blocker as last sibling
                ShowBackgroundBlocker();
                
                // Set screen as last sibling so the bg is behind it
                screen.transform.SetAsLastSibling();
            }
            else
            {
                // Open the panel
                screen.Open(properties, () =>
                {
                    // Unblock ui interactions
                    RequestUIInteractionUnblock?.Invoke();
                });
            }

            return screen;
        }
        
        internal void CloseScreen<T>() where T : UiScreenBase
        {
            CloseScreen(typeof(T));
        }
        
        internal void CloseScreen(Type screenType)
        {
            var screen = GetScreen(screenType);

            if (screen == null)
            {
                Debug.LogError($"Screen is not instantiated, can not close: {screenType}", LogContext.UIFrame);
                return;
            }
            
            if (screen.GetState() != ScreenState.Opened)
            {
                Debug.LogError($"Screen is not in 'Opened' state, can not close: {screenType}", LogContext.UIFrame);
                return;
            }
            
            // Request ui interaction block while closing the screen
            RequestUIInteractionBlock?.Invoke();
            
            if (_layerInfo.LayerType == LayerType.Popup)
            {
                screen.Close(() =>
                {
                    screen.transform.SetAsFirstSibling();
                    
                    // Unblock ui interactions
                    RequestUIInteractionUnblock?.Invoke();
                    
                    TryDestroyOnClose(screenType);
                    
                    // Refresh bg visibility
                    RefreshBackgroundBlocker();
                });
            }
            else
            {
                screen.Close(() =>
                {
                    // Unblock ui interactions
                    RequestUIInteractionUnblock?.Invoke();
                    
                    TryDestroyOnClose(screenType);
                });
            }
        }

        private void TryDestroyOnClose(Type screenType)
        {
            var screenInfo = GetScreenInfo(screenType);
            if(!screenInfo.DestroyOnClose) return;
            var screen = GetScreen(screenType);
            Destroy(screen.gameObject);
            _screensByType[screenType] = null;
        }

        private ScreenInfo GetScreenInfo(Type screenType)
        {
            return _screenInfosByType.GetValueOrDefault(screenType);
        }
        
        private void ShowBackgroundBlocker()
        {
            _backgroundBlocker.gameObject.SetActive(true);
            _backgroundBlocker.gameObject.transform.SetAsLastSibling();
        }

        private void RefreshBackgroundBlocker()
        {
            var visibleScreens = GetVisibleScreens();

            if (visibleScreens.Count == 0) 
            {
                _backgroundBlocker.gameObject.SetActive(false);
            }
            else
            {
                var bgIndex = _backgroundBlocker.transform.GetSiblingIndex();
                _backgroundBlocker.transform.SetSiblingIndex(bgIndex - 1);
            }
        }

        private UiScreenBase GetScreen(Type screenType)
        {
            return _screensByType.TryGetValue(screenType, out var screen) ? screen : null;
        }
        
        internal UiScreenBase GetScreen<T>() where T : UiScreenBase
        {
            return GetScreen(typeof(T));
        }

        internal List<Type> GetAllScreenTypes()
        {
            return _screensByType.Keys.ToList();
        }
        
        internal LayerInfo GetLayerInfo()
        {
            return _layerInfo;
        }
        
        [PublicAPI] public List<UiScreenBase> GetVisibleScreens()
        {
            return _screensByType.Values.Where(screen => screen != null && screen.GetState() != ScreenState.Closed).ToList();
        }

        [PublicAPI] public bool IsAnyScreenVisible()
        {
            return GetVisibleScreens().Count > 0;
        }
        
        [PublicAPI] public List<UiScreenBase> GetAllScreens()
        {
            return _screensByType.Values.Where(screen => screen != null).ToList();
        }
    }
}