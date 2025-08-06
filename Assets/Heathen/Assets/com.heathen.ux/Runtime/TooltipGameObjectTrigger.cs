#if HE_SYSCORE

using UnityEngine;
using UnityEngine.EventSystems;

namespace HeathenEngineering.UX
{
    /// <summary>
    /// A simple Tooltip Trigger tool that sets the state of a configured game object to match the state of the trigger
    /// </summary>
    [AddComponentMenu("UX/Tooltip/Tooltip GameObject Trigger")]
    public class TooltipGameObjectTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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

        [Header("Target")]
        public GameObject tip;

        private float enterTime;
        private bool hasMouse;

        public void OnPointerEnter(PointerEventData eventData)
        {
            enterTime = Time.unscaledTime;
            hasMouse = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "<Pending>")]
        public void OnPointerExit(PointerEventData eventData)
        {
            tip?.SetActive(false);
            hasMouse = false;
        }

        private void Update()
        {
            if (isActive.Value)
            {
                if (hasMouse && Time.unscaledTime - enterTime >= triggerDelay)
                {
                    if (useCancelTimer.Value && Time.unscaledTime - enterTime > cancelDelay + triggerDelay)
                    {
                        if(tip && tip.activeSelf)
                        tip.SetActive(false);
                    }
                    else if (tip && !tip.activeSelf)
                        tip.SetActive(true);
                }
                else
                {
                    if (tip && tip.activeSelf)
                        tip.SetActive(false);
                }
            }
        }

        private void OnEnable()
        {
            var rectTrans = GetComponent<RectTransform>();
            var canvas = GetComponentInParent<Canvas>();
            if(rectTrans != null)
            {
                if(canvas != null)
                {
                    if(canvas.renderMode == RenderMode.ScreenSpaceOverlay)
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
            tip?.SetActive(false);
            hasMouse = false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "<Pending>")]
        private void OnDestroy()
        {
            tip?.SetActive(false);
            hasMouse = false;
        }
    }
}

#endif