using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace X1Frameworks.UiFramework
{
    public enum LayerType
    {
        Panel = 0,
        Popup = 1,
    }
    [Serializable] 
    public class ScreenInfo
    {
        public UiScreenBase Prefab;
        public bool LoadOnDemand;
        public bool DestroyOnClose;
        public bool CloseWithEscape;
        public bool CloseWithBgClick;
    }
    [Serializable] 
    public class LayerInfo
    {
        public string Name;
        public LayerType LayerType;

        [NonSerialized] public Color BackgroundBlockerColor;
        
        public List<ScreenInfo> Screens;
    }
    
    [CreateAssetMenu(fileName = "UISettings", menuName = "X1 Frameworks/UI Frameworks/UiSettings")]
    public class UiSettings : ScriptableObject
    {
        [Header("Canvas Settings")]
        public RenderMode renderMode = RenderMode.ScreenSpaceOverlay;
        public string sortingLayerName = "UI";
        public int orderInLayer = 10000;
        
        [Header("CanvasScaler Settings")]
        public CanvasScaler.ScreenMatchMode screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        public Vector2 referenceResolution = new Vector2(1080, 1920);
        public float referencePixelsPerUnit = 100;
        [Range(0f, 1f)] public float matchWidthOrHeight;

        [Header("Background Blocker")] 
        public Color backgroundBlockerColor = new Color(0f, 0f, 0f, 0.75f);
        
        [Header("Layers")]
        public List<LayerInfo> layers;
        
        public UIFrame BuildUIFrame()
        {
            var root = new GameObject("[UIFrame]");
            root.layer = LayerMask.NameToLayer("UI");;
            
            // Canvas
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = renderMode;
            canvas.sortingOrder = orderInLayer;
            canvas.sortingLayerName = sortingLayerName;
            canvas.sortingLayerID = SortingLayer.NameToID(sortingLayerName);

            // Canvas scaler
            var canvasScaler = root.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = referenceResolution;
            canvasScaler.screenMatchMode = screenMatchMode;
            canvasScaler.referencePixelsPerUnit = referencePixelsPerUnit;
            canvasScaler.matchWidthOrHeight = matchWidthOrHeight;

            // Graphic raycaster
            var graphicRaycaster = root.AddComponent<GraphicRaycaster>();
            
            // UI Frame
            var uiFrame = root.AddComponent<UIFrame>();
            uiFrame.Construct(this, canvas);
            return uiFrame;
        }
        
    }
}
