using System;
using UnityEngine;

namespace X1Frameworks.UiFramework
{
    public interface IScreenProperties { }

    public enum ScreenState
    {
        Opening,
        Opened,
        Closing,
        Closed
    }
    
    public enum OnScreenEvent
    {
        Created,
        Opening,
        Opened,
        Closing,
        Closed,
        Destoryed,
        MAX
    }
    public abstract class UiScreenBase : MonoBehaviour
    {
        public UITransition transition;
        internal Action<OnScreenEvent,UiScreenBase> OnScreenEvent;
        protected ScreenState _screenState = ScreenState.Closed;
        internal Action<Type> CloseRequest { get; set; }
        internal abstract void InitScreen();
        internal abstract void Open(IScreenProperties props = null, Action onTransitionCompleteCallback = null);
        internal abstract void Close(Action onTransitionCompleteCallback = null);
        internal abstract ScreenState GetState();
    }

    public abstract class UIScreen<TProps> : UiScreenBase where TProps : IScreenProperties
    {
        [NonSerialized] protected TProps Properties;
        protected virtual void OnCreated() {}
        protected virtual void OnOpening() {}
        protected virtual void OnOpened() {}
        protected virtual void OnClosing() {}
        protected virtual void OnClosed() { }
        protected virtual void OnDestroyed() {}
        internal override void InitScreen()
        {
            OnCreated();
        }
        private void OnDestroy()
        {
            OnScreenEvent?.Invoke(UiFramework.OnScreenEvent.Destoryed, this);
            OnDestroyed();
        }

        internal override void Open(IScreenProperties props = null, Action onTransitionCompleteCallback = null)
        {
            if (props != null)
            {
                if (props is TProps tProps)
                {
                    Properties = tProps;
                }
                else
                {
                    Debug.LogError(
                        "UIFrame Properties passed have wrong type! (" + props.GetType() + " instead of " + typeof(TProps) + ")");
                    return;
                }
            }
            
            if (_screenState != ScreenState.Closed)
            {
                Debug.LogWarning(
                    "UIFrame Screen is already visible, can not open: " + GetType());
                return;
            }
            
            gameObject.SetActive(true);
            _screenState = ScreenState.Opening;
            
            OnScreenEvent?.Invoke(UiFramework.OnScreenEvent.Opening,this);
            OnOpening();
            
            DoAnimation(transition, () =>
            {
                // Set state
                _screenState = ScreenState.Opened;
                
                // Call OnOpened when transition finishes
                OnScreenEvent?.Invoke(UiFramework.OnScreenEvent.Opened,this);
                OnOpened();
                
                // Animation complete callback
                onTransitionCompleteCallback?.Invoke();
            }, true);
        }
        
        private void DoAnimation(UITransition transition, Action callWhenFinished, bool isOpeningAnimation)
        {
            if (transition == null)
            {
                // No transition animation defined
                // Set immediately and invoke callback
                gameObject.SetActive(isOpeningAnimation);
                callWhenFinished?.Invoke();
            }
            else
            {
                // Start animation
                if (isOpeningAnimation)
                {
                    transition.AnimateOpen(transform, callWhenFinished);
                }
                else
                {
                    transition.AnimateClose(transform, callWhenFinished);
                }
            }
        }

        internal override void Close(Action onTransitionCompleteCallback = null)
        {
            if (_screenState != ScreenState.Opened)
            {
                Debug.LogWarning("UIFrame Screen is not visible, can not close: " + GetType());
                return;
            }
            _screenState = ScreenState.Closing;
            OnScreenEvent?.Invoke(UiFramework.OnScreenEvent.Closing,this);
            OnClosing();
            
            DoAnimation(transition, () =>
            {
                // Disable game object
                gameObject.SetActive(false);

                // Set state
                _screenState = ScreenState.Closed;
                
                // Call OnClosed when transition finishes
                OnScreenEvent?.Invoke(UiFramework.OnScreenEvent.Closed,this);
                OnClosed();
                
                // Animation complete callback
                onTransitionCompleteCallback?.Invoke();
            }, false);
        }
        
        internal override ScreenState GetState()
        {
            return _screenState;
        }
        
        protected void ForceClose()
        {
            CloseRequest?.Invoke(GetType());
        }
    }
}
