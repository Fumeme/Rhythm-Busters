#if HE_SYSCORE

using HeathenEngineering.Events;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HeathenEngineering.UX
{
    /// <summary>
    /// A Tooltip Trigger tool that raises GameEvents as the trigger state changes
    /// </summary>
    [AddComponentMenu("UX/Tooltip/Tooltip Game Event Trigger")]
    public class TooltipGameEventTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Settings")]
        [Tooltip("Is the trigger active e.g. will it invoke or ignore.")]
        public BoolReference isActive = new BoolReference(true);
        [Tooltip("How long before the trigger is invoked if active.")]
        public FloatReference triggerDelay = new FloatReference(2);
        [Tooltip("Should the trigger cancel after invokation and the timer expiers.")]
        public BoolReference useCancelTimer = new BoolReference(true);
        [Tooltip("If the trigger should cancel how long should it wait to do so.")]
        public FloatReference cancelDelay = new FloatReference(5);

        [Header("Events")]
        public GameEvent Invoked;
        public GameEvent Canceled;

        private float enterTime;
        private bool hasMouse;
        private bool isInvoked;

        public void OnPointerEnter(PointerEventData eventData)
        {
            enterTime = Time.unscaledTime;
            hasMouse = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isInvoked)
                Canceled?.Invoke();
            hasMouse = false;

            isInvoked = false;
        }

        private void Update()
        {
            if (isActive.Value)
            {
                if (hasMouse && Time.unscaledTime - enterTime >= triggerDelay)
                {
                    if (isInvoked && useCancelTimer.Value && Time.unscaledTime - enterTime > cancelDelay + triggerDelay)
                    {
                        if (Canceled != null)
                        {
                            Canceled.Invoke();
                        }
                    }
                    else if (Invoked != null && !isInvoked)
                    {
                        Invoked.Invoke();
                        isInvoked = true;
                    }
                }
            }
        }

        private void OnEnable()
        {
            var rectTrans = GetComponent<RectTransform>();
            var canvas = GetComponentInParent<Canvas>();
            if (rectTrans != null)
            {
                if (canvas != null)
                {
                    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
#if ENABLE_LEGACY_INPUT_MANAGER
                        hasMouse = RectTransformUtility.RectangleContainsScreenPoint(rectTrans, Input.mousePosition);
#else
                        hasMouse = RectTransformUtility.RectangleContainsScreenPoint(rectTrans, UnityEngine.InputSystem.Mouse.current.position.ReadValue());
#endif
                    }
                    else
                    {
                        var camera = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
#if ENABLE_LEGACY_INPUT_MANAGER
                        hasMouse = RectTransformUtility.RectangleContainsScreenPoint(rectTrans, Input.mousePosition, camera);
#else
                        hasMouse = RectTransformUtility.RectangleContainsScreenPoint(rectTrans, UnityEngine.InputSystem.Mouse.current.position.ReadValue(), camera);
#endif
                    }
                }
                else
                {
#if ENABLE_LEGACY_INPUT_MANAGER
                    hasMouse = RectTransformUtility.RectangleContainsScreenPoint(rectTrans, Input.mousePosition);
#else
                    hasMouse = RectTransformUtility.RectangleContainsScreenPoint(rectTrans, UnityEngine.InputSystem.Mouse.current.position.ReadValue());
#endif
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "<Pending>")]
        private void OnDisable()
        {
            if (isInvoked && Canceled != null)
                Canceled.Invoke();

            hasMouse = false;
            isInvoked = false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "<Pending>")]
        private void OnDestroy()
        {
            if (isInvoked && Canceled != null)
                Canceled.Invoke();

            hasMouse = false;
            isInvoked = false;
        }
    }
}

#endif