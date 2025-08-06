#if HE_SYSCORE

using HeathenEngineering.Events;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.UX
{
#if !ENABLE_LEGACY_INPUT_MANAGER
    [System.Obsolete("Only applicable when the legacy Unity Input System is enabled")]
#endif
    /// <summary>
    /// Handles a button press event where the user must press and hold that button for a given time
    /// </summary>
    public class KeyHoldUnityEvent : MonoBehaviour
    {
#if ENABLE_LEGACY_INPUT_MANAGER

        /// <summary>
        /// The input action to use when testing the process
        /// </summary>
        public KeyCode[] keys;
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
        public UnityEvent evtStart;
        /// <summary>
        /// Invoked when the start button is released early or if the pointer leaves the control
        /// </summary>
        public UnityEvent evtCancel;
        /// <summary>
        /// Invoked when the hold is completed successfuly
        /// </summary>
        public UnityEvent evtComplete;
        /// <summary>
        /// Invoked each frame while the click is being held and updates the progress as a value between 0 and 1
        /// </summary>
        public UnityFloatEvent evtProgressed;

        private bool pressed = false;
        private bool working = false;
        private float startTime = 0;

        private void Update()
        {
            var allHeld = true;

            foreach(var key in keys)
            {
                if(!Input.GetKey(key))
                {
                    allHeld = false;
                    break;
                }
            }

            if(allHeld)
            {
                if (!working && !pressed)
                {
                    pressed = true;
                    working = true;
                    startTime = useUnscaledTime.Value ? Time.unscaledTime : Time.time;
                    evtStart.Invoke();
                }
            }
            else
            {
                if (working)
                    evtCancel.Invoke();

                working = false;
                pressed = false;
            }

            if (working)
            {
                float t;

                if (useUnscaledTime.Value)
                    t = Time.unscaledTime - startTime;
                else
                    t = Time.time - startTime;

                if (t >= holdTime)
                {
                    working = false;
                    evtProgressed.Invoke(1f);
                    evtComplete.Invoke();
                }
                else
                {
                    evtProgressed.Invoke(t / holdTime.Value);
                }
            }
        }
#endif
    }
}

#endif