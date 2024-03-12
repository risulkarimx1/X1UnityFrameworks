using System;
using DG.Tweening;
using UnityEngine;

namespace X1Frameworks.UiFramework
{
    [RequireComponent(typeof(CanvasGroup))]
    public class SimpleFadeTransition: UITransition
    {
        [Header("Open")] 
        [SerializeField] private float openDuration = 0.15f;
        [SerializeField] private Ease openEase = Ease.OutSine;

        [Header("Close")] 
        [SerializeField] private float closeDuration = 0.15f;
        [SerializeField] private Ease closeEase = Ease.InSine;

        public override void AnimateOpen(Transform target, Action onTransitionCompleteCallback)
        {
            var canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, openDuration)
                    .SetEase(openEase)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        onTransitionCompleteCallback?.Invoke();
                    });
            }
            else
            {
                onTransitionCompleteCallback?.Invoke();
            }
        }

        public override void AnimateClose(Transform target, Action onTransitionCompleteCallback)
        {
            var canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
                canvasGroup.alpha = 1f;
                canvasGroup.DOFade(0f, closeDuration)
                    .SetEase(closeEase)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        onTransitionCompleteCallback?.Invoke();
                    });
            }
            else
            {
                onTransitionCompleteCallback?.Invoke();
            }
        }
    }
}