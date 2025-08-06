#if HE_SYSCORE

using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HeathenEngineering.UX.uGUI
{
    /// <summary>
    /// To be attached to a window for resizing of it.
    /// </summary>
    /// <remarks>
    /// A handle for changing the size of a window
    /// </remarks>
    public class BorderHandle : MonoBehaviour, IPointerDownHandler
    {
        public GrabHandle handle;

        private Window parentWindow;
        private Canvas parentCanvas;
        private Camera eventCamera;
        private bool mouseDown = false;
        private Vector2 localPoint;
        private Vector2 halfScreen;
        private Vector2 sizeAtStart;

        private void OnEnable()
        {
            parentWindow = GetComponentInParent<Window>();

            if (parentWindow == null)
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
            sizeAtStart = parentWindow.SelfTransform.rect.size;
            parentWindow.OnPointerDown(eventData);
        }

        private void LateUpdate()
        {
            halfScreen.x = Screen.width / 2f;
            halfScreen.y = Screen.height / 2f;

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
                Vector2 targetPosition = parentWindow.SelfTransform.localPosition;
                Vector2 targetSize = sizeAtStart;

                Vector2 mousePos;
                Vector2 delta;
                Vector2 change;
                
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentWindow.SelfTransform, mousePosition2d, parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : eventCamera, out mousePos);
                
                switch (handle)
                {
                    case GrabHandle.Left:
                        delta = mousePos - localPoint;
                        change = sizeAtStart - delta;
                        if (change.x > parentWindow.minimalSize.Value.x)
                        {
                            targetPosition = new Vector2(parentWindow.Position.x + delta.x * (1 - parentWindow.SelfTransform.pivot.x), parentWindow.Position.y);
                            targetSize = new Vector2(change.x, sizeAtStart.y);
                        }
                        else
                            return;
                        break;
                    case GrabHandle.Right:
                        delta = mousePos - localPoint;
                        delta.x *= -1;
                        change = sizeAtStart - delta;
                        if (change.x > parentWindow.minimalSize.Value.x)
                        {
                            targetPosition = new Vector2(parentWindow.Position.x - delta.x * (parentWindow.SelfTransform.pivot.x), parentWindow.Position.y);
                            targetSize = new Vector2(change.x, sizeAtStart.y);
                        }
                        else
                            return;
                        break;
                    case GrabHandle.Top:
                        delta = mousePos - localPoint;
                        delta.y *= -1;
                        change = sizeAtStart - delta;
                        if (change.y > parentWindow.minimalSize.Value.y)
                        {
                            targetPosition = new Vector2(parentWindow.Position.x, parentWindow.Position.y - delta.y * (parentWindow.SelfTransform.pivot.y));
                            targetSize = new Vector2(sizeAtStart.x, change.y);
                        }
                        else
                            return;
                        break;
                    case GrabHandle.Bottom:
                        delta = mousePos - localPoint;
                        change = sizeAtStart - delta;
                        if (change.y > parentWindow.minimalSize.Value.y)
                        {
                            targetPosition = new Vector2(parentWindow.Position.x, parentWindow.Position.y + delta.y * (1 - parentWindow.SelfTransform.pivot.y));
                            targetSize = new Vector2(sizeAtStart.x, change.y);
                        }
                        else
                            return;
                        break;
                    case GrabHandle.UpperLeft:
                        delta = mousePos - localPoint;
                        delta.y *= -1;
                        change = sizeAtStart - delta;
                        if (change.x > parentWindow.minimalSize.Value.x && change.y > parentWindow.minimalSize.Value.y)
                        {
                            targetPosition = new Vector2(parentWindow.Position.x + delta.x * (1 - parentWindow.SelfTransform.pivot.x), parentWindow.Position.y - delta.y * (parentWindow.SelfTransform.pivot.y));
                            targetSize = new Vector2(change.x, change.y);
                        }
                        else if (change.x > parentWindow.minimalSize.Value.x)
                        {
                            targetPosition = new Vector2(parentWindow.Position.x + delta.x * (1 - parentWindow.SelfTransform.pivot.x), parentWindow.Position.y);
                            targetSize = new Vector2(change.x, sizeAtStart.y);
                        }
                        else if (change.y > parentWindow.minimalSize.Value.y)
                        {
                            targetPosition = new Vector2(parentWindow.Position.x, parentWindow.Position.y - delta.y * (parentWindow.SelfTransform.pivot.y));
                            targetSize = new Vector2(sizeAtStart.x, change.y);
                        }
                        else
                            return;
                        break;
                    case GrabHandle.UpperRight:
                        delta = mousePos - localPoint;
                        delta *= -1;
                        change = sizeAtStart - delta;
                        if (change.x > parentWindow.minimalSize.Value.x && change.y > parentWindow.minimalSize.Value.y)
                        {
                            targetPosition = new Vector2(parentWindow.Position.x - delta.x * (parentWindow.SelfTransform.pivot.x), parentWindow.Position.y - delta.y * (parentWindow.SelfTransform.pivot.y));
                            targetSize = new Vector2(change.x, change.y);
                        }
                        else if (change.x > parentWindow.minimalSize.Value.x)
                        {
                            targetPosition = new Vector2(parentWindow.Position.x - delta.x * (parentWindow.SelfTransform.pivot.x), parentWindow.Position.y);
                            targetSize = new Vector2(change.x, sizeAtStart.y);
                        }
                        else if (change.y > parentWindow.minimalSize.Value.y)
                        {
                            targetPosition = new Vector2(parentWindow.Position.x, parentWindow.Position.y - delta.y * (parentWindow.SelfTransform.pivot.y));
                            targetSize = new Vector2(sizeAtStart.x, change.y);
                        }
                        else
                            return;
                        break;
                    case GrabHandle.LowerLeft:
                        delta = mousePos - localPoint;
                        change = sizeAtStart - delta;
                        if (change.x > parentWindow.minimalSize.Value.x && change.y > parentWindow.minimalSize.Value.y)
                        {
                            targetPosition = new Vector2(parentWindow.Position.x + delta.x * (1 - parentWindow.SelfTransform.pivot.x), parentWindow.Position.y + delta.y * (1 - parentWindow.SelfTransform.pivot.y));
                            targetSize = new Vector2(change.x, change.y);
                        }
                        else if (change.x > parentWindow.minimalSize.Value.x)
                        {
                            targetPosition = new Vector2(parentWindow.Position.x + delta.x * (1 - parentWindow.SelfTransform.pivot.x), parentWindow.Position.y);
                            targetSize = new Vector2(change.x, sizeAtStart.y);
                        }
                        else if (change.y > parentWindow.minimalSize.Value.y)
                        {
                            targetPosition = new Vector2(parentWindow.Position.x, parentWindow.Position.y + delta.y * (1 - parentWindow.SelfTransform.pivot.y));
                            targetSize = new Vector2(sizeAtStart.x, change.y);
                        }
                        else
                            return;
                        break;
                    case GrabHandle.LowerRight:
                        delta = mousePos - localPoint;
                        delta.x *= -1;
                        change = sizeAtStart - delta;
                        if (change.x > parentWindow.minimalSize.Value.x && change.y > parentWindow.minimalSize.Value.y)
                        {
                            targetPosition = new Vector2(parentWindow.Position.x - delta.x * (parentWindow.SelfTransform.pivot.x), parentWindow.Position.y + delta.y * (1 - parentWindow.SelfTransform.pivot.y));
                            targetSize = new Vector2(change.x, change.y);
                        }
                        else if (change.x > parentWindow.minimalSize.Value.x)
                        {
                            targetPosition = new Vector2(parentWindow.Position.x - delta.x * (parentWindow.SelfTransform.pivot.x), parentWindow.Position.y);
                            targetSize = new Vector2(change.x, sizeAtStart.y);
                        }
                        else if (change.y > parentWindow.minimalSize.Value.y)
                        {
                            targetPosition = new Vector2(parentWindow.Position.x, parentWindow.Position.y + delta.y * (1 - parentWindow.SelfTransform.pivot.y));
                            targetSize = new Vector2(sizeAtStart.x, change.y);
                        }
                        else
                            return;
                        break;
                }

                var changes = parentWindow.SetTransfrom(targetPosition, targetSize);
                var cashe = localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentWindow.SelfTransform, mousePosition2d, parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : eventCamera, out localPoint);
                
                if (changes.x < 1)
                    localPoint.x = cashe.x;
                if (changes.y < 1)
                    localPoint.y = cashe.y;

                sizeAtStart = parentWindow.SelfTransform.rect.size;
            }
        }
    }
}


#endif