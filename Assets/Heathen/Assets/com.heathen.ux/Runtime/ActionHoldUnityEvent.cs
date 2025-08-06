#if HE_SYSCORE
using HeathenEngineering.Events;
using System;
using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HeathenEngineering.UX
{
#if !ENABLE_INPUT_SYSTEM
    [System.Obsolete("Only applicable when the new Unity Input System is enabled")]
#endif
    /// <summary>
    /// Handles a button press event where the user must press and hold that button for a given time
    /// </summary>
    public class ActionHoldUnityEvent : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        [Serializable]
        public class UnityInputActionCallbackContextEvent : UnityEvent<InputAction.CallbackContext>
        { }

        /// <summary>
        /// The input action to use when testing the process
        /// </summary>
        public InputAction action;
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
        public UnityInputActionCallbackContextEvent evtStart;
        /// <summary>
        /// Invoked when the start button is released early or if the pointer leaves the control
        /// </summary>
        public UnityInputActionCallbackContextEvent evtCancel;
        /// <summary>
        /// Invoked when the hold is completed successfuly
        /// </summary>
        public UnityEvent evtComplete;
        /// <summary>
        /// Invoked each frame while the click is being held and updates the progress as a value between 0 and 1
        /// </summary>
        public UnityFloatEvent evtProgressed;

        private bool working = false;
        private float startTime = 0;

        private void Awake()
        {
            action.started += OnStarted;
            action.canceled += OnCanceled;
        }

        private void OnCanceled(InputAction.CallbackContext obj)
        {
            working = false;
            evtCancel.Invoke(obj);
        }

        private void OnStarted(InputAction.CallbackContext obj)
        {
            working = true;
            startTime = useUnscaledTime.Value ? Time.unscaledTime : Time.time;
            evtStart.Invoke(obj);
        }

        private void OnEnable()
        {
            if (action.type != UnityEngine.InputSystem.InputActionType.Button)
            {
                Debug.LogWarning("The Press and Hold componenet requires a button type action");
                enabled = false;
                return;
            }

            action.Enable();
        }

        private void OnDisable()
        {
            action.Disable();
        }

        private void Update()
        {
            if(working)
            {
                float t;

                if (useUnscaledTime.Value)
                    t = Time.unscaledTime - startTime;
                else
                    t = Time.time - startTime;

                if(t >= holdTime)
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