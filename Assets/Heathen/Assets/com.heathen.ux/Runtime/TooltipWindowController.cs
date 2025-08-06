#if HE_SYSCORE

using UnityEngine;
using UnityEngine.EventSystems;

namespace HeathenEngineering.UX
{
    /// <summary>
    /// Attach to a RECT to cause it to remain open while the mouse pointer is insider
    /// </summary>
    public class TooltipWindowController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private bool triggerActive = false;
        private bool hasMouse;

        public void OnPointerEnter(PointerEventData eventData)
        {
            hasMouse = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hasMouse = false;

            if (!triggerActive)
                gameObject.SetActive(false);
        }

        /// <summary>
        /// To be used by the Tooltip Trigger system to indicate tip triggered
        /// </summary>
        public void TriggerInvoked()
        {
            triggerActive = true;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// To be used by the Tooltip Trigger system to indicate the trigger has closed
        /// </summary>
        public void TriggerCanceled()
        {
            triggerActive = false;

            //We simply dont close if the window currently has the mouse pointer
            if(!hasMouse)
                gameObject.SetActive(false);
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

        private void OnDisable()
        {
            hasMouse = false;
        }

        private void OnDestroy()
        {
            hasMouse = false;
        }
    }
}


#endif