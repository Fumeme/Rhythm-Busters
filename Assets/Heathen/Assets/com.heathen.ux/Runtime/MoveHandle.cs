#if HE_SYSCORE

using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HeathenEngineering.UX.uGUI
{
    public class MoveHandle : HeathenUIBehaviour, IPointerDownHandler
    {
        private Window parentWindow;
        private Canvas parentCanvas;
        private Camera eventCamera;
        private bool mouseDown = false;
        private Vector2 localPoint;

        private void OnEnable()
        {
            parentWindow = GetComponentInParent<Window>();

            if(parentWindow == null)
            {
                Debug.LogWarning("GrabHandle requires a DynamicRect parent.\nThe behaviour will be disabled.");
                enabled = false;
                return;
            }

            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null)
                eventCamera = parentCanvas.worldCamera != null ? parentCanvas.worldCamera : Camera.main;
            else
                eventCamera = Camera.main;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            mouseDown = true;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentWindow.SelfTransform, eventData.position, parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : eventCamera, out localPoint);
            parentWindow.OnPointerDown(eventData);
        }

        private void LateUpdate()
        {
#if ENABLE_INPUT_SYSTEM
            // New input system backends are enabled.
            if (mouseDown && !Mouse.current.leftButton.isPressed)
                mouseDown = false;

            var mousePosition2d = Mouse.current.position.ReadValue();
#else
            // Old input backends are enabled.
            if (mouseDown && !Input.GetMouseButton(0))
                mouseDown = false;

            var mousePosition2d = (Vector2)Input.mousePosition;
#endif

            if (mouseDown)
            {
                Vector2 delta;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentWindow.SelfTransform, mousePosition2d, parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : eventCamera, out delta);
                Vector3 positionChange = (delta - localPoint);
                parentWindow.Move(parentWindow.SelfTransform.localPosition + positionChange); 
            }
        }
    }
}

#endif