#if HE_SYSCORE

using HeathenEngineering.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
#endif

namespace HeathenEngineering.UX
{
    /// <summary>
    /// Handles pointer events where the user must click and hold for a given time
    /// </summary>
    public class PointerHoldUnityEvent : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler
    {
        /// <summary>
        /// Should the system test for left clicks
        /// </summary>
        public BoolReference handleLeftClick = new BoolReference(true);
        /// <summary>
        /// Should the system test for right clicks
        /// </summary>
        public BoolReference handleRightClick = new BoolReference(true);
        /// <summary>
        /// Should the system test for middle clicks
        /// </summary>
        public BoolReference handleMiddleClick = new BoolReference(true);
        /// <summary>
        /// How long in seconds must the user hold the click
        /// </summary>
        public FloatReference holdTime = new FloatReference(1);
        /// <summary>
        /// Should the system test time as an unscaled value
        /// </summary>
        public BoolReference useUnscaledTime = new BoolReference(true);
        [Header("Events")]
        /// <summary>
        /// Invoked when a valid mouse down is detected
        /// </summary>
        public UnityPointerEvent evtClickStart;
        /// <summary>
        /// Invoked when the start button is released early or if the pointer leaves the control
        /// </summary>
        public UnityPointerEvent evtClickCancel;
        /// <summary>
        /// Invoked when the hold is completed successfuly
        /// </summary>
        public UnityEvent evtClickComplete;
        /// <summary>
        /// Invoked each frame while the click is being held and updates the progress as a value between 0 and 1
        /// </summary>
        public UnityFloatEvent evtClickProgressed;

        private bool hasMouse = false;
        private float startTime;
        private PointerEventData.InputButton button;

        public void OnPointerDown(PointerEventData eventData)
        {
            if ( !hasMouse
                && ((eventData.button == PointerEventData.InputButton.Left && handleLeftClick)
                    || (eventData.button == PointerEventData.InputButton.Middle && handleMiddleClick)
                    || (eventData.button == PointerEventData.InputButton.Right && handleRightClick)))
            {
                hasMouse = true;
                button = eventData.button;

                if (useUnscaledTime.Value)
                    startTime = Time.unscaledTime;
                else
                    startTime = Time.time;

                evtClickStart.Invoke(eventData);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(hasMouse)
            {
                hasMouse = false;

                evtClickCancel.Invoke(eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if(hasMouse && eventData.button == button)
            {
                hasMouse = false;

                evtClickCancel.Invoke(eventData);
            }
        }

        private void Update()
        {
            if (hasMouse)
            {
                float t;

                if (useUnscaledTime.Value)
                    t = Time.unscaledTime - startTime;
                else
                    t = Time.time - startTime;

                if (t >= holdTime.Value)
                {
                    hasMouse = false;

                    evtClickProgressed.Invoke(1f);
                    evtClickComplete.Invoke();
                }
                else
                {
                    evtClickProgressed.Invoke(t / startTime);
                }
            }
        }
    }
}

#endif