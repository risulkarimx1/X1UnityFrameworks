using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace X1Frameworks.UiFramework
{
    public class PopupBackgroundBlocker : MonoBehaviour, IPointerDownHandler
    {
        public event Action OnBackgroundBlockerClick;

        public void Init(Transform parent, Color color)
        {
            // Add rect transform and set anchors
            var rt = gameObject.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            
            // Add image and set color
            var image = gameObject.AddComponent<Image>();
            image.color = color;
            transform.SetParent(parent, false);
            
            // Set rect transform size delta to ensure full stretched
            rt.sizeDelta = Vector3.zero;
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            OnBackgroundBlockerClick?.Invoke();
        }
    }
}