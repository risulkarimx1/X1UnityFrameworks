using System;
using UnityEngine;

namespace X1Frameworks.UiFramework
{
    public abstract class UITransition : MonoBehaviour
    {
        public abstract void AnimateOpen(Transform target, Action onTransitionCompleteCallback);
        public abstract void AnimateClose(Transform target, Action onTransitionCompleteCallback);
    }
}