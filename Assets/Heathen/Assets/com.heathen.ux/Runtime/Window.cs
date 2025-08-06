#if HE_SYSCORE

using HeathenEngineering.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace HeathenEngineering.UX
{
    /// <summary>
    /// Represents a Window Rect
    /// </summary>
    /// <remarks>
    /// <para>
    /// A window has a number of unique features in particular exsactly 1 window can be "focused" as indicated by the <see cref="Window.focused"/> static member.
    /// When a window is focused it will rise to the top of its branch of transforms.
    /// A window can be "docked" to any RectTransform, this is simply the process of setting the position and size to match it does not change parantage.
    /// A window may have handles that allow it to be resized or moved via mouse input.
    /// Movement and resizing will not exceed the parent RectTransform
    /// </para>
    /// <para>
    /// This implamentation allows the designer to use any anchor and pivot set up they like calculations are not dependent on the windows RectTransform settings.
    /// </para>
    /// </remarks>
    public class Window : HeathenUIBehaviour, IPointerDownHandler
    {
        #region Obsolete
        [System.Obsolete("Use API.Windows.focused")]
        public static Window Focused => API.Windows.Focused;
        [System.Obsolete("Use SetTransform")]
        public void DockTo(RectTransform target) => API.Windows.SetTransfrom(this, target);
        [System.Obsolete("Use SetTransform")]
        public void DockTo(Vector2 size, Vector3 position) => SetTransfrom(position, size);
        #endregion

        [Header("Configuration")]
        [Tooltip("The minimal size the window is permited to take based on resize events.")]
        public Vector2Reference minimalSize = new Vector2Reference(new Vector2(100, 50));
        [Tooltip("Should this window always be the top most in its parent rect.")]
        public BoolReference alwaysOnTop = new BoolReference(false);
        [Tooltip("Should the window be forced to remain in the bounds of the screen")]
        public BoolReference clampToParent = new BoolReference(true);

        [Header("Game Events")]
        [Tooltip("A Game Event to be invoked when the window takes focus")]
        [FormerlySerializedAs("FocusedEvent")]
        [Label("Focused")]
        public GameEvent gevtFocused;
        [Tooltip("A Unity Event to be invoked when the window loses focus")]
        [Label("Lost Focus")]
        public GameEvent gevtLostFocus;
        [Tooltip("A Unity Event to be invoked when the window is maximized")]
        [Label("Maximized")]
        public GameEvent gevtMaximized;
        [Tooltip("A Unity Event to be invoked when the window is minimized")]
        [Label("Minimized")]
        public GameEvent gevtMinimized;
        [Tooltip("A Unity Event to be invoked when the window's minimized or maximized states are broken")]
        [Label("Restored")]
        public GameEvent gevtRestored;

        [Header("Unity Events")]
        [Tooltip("A Unity Event to be invoked when the window takes focus")]
        public UnityEvent evtFocused;
        [Tooltip("A Unity Event to be invoked when the window loses focus")]
        public UnityEvent evtLostFocus;
        [Tooltip("A Unity Event to be invoked when the window is maximized")]
        public UnityEvent evtMaximized;
        [Tooltip("A Unity Event to be invoked when the window is minimized")]
        public UnityEvent evtMinimized;
        [Tooltip("A Unity Event to be invoked when the window's minimized or maximized states are broken")]
        public UnityEvent evtRestored;

        public bool HasFocus
        {
            get => API.Windows.Focused == this;
            set
            {
                if (value)
                    Focus();
                else if (API.Windows.Focused == this)
                    API.Windows.Focused = null;
            }
        }

        /// <summary>
        /// Get or set the position of the window
        /// </summary>
        /// <remarks>
        /// This respects the clamp to parent value
        /// </remarks>
        public Vector2 Position
        {
            get => SelfTransform.localPosition;
            set => Move(value);
        }

        /// <summary>
        /// Gets or set the width of the window
        /// </summary>
        /// <remarks>
        /// This respects the minimal size x value
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "<Pending>")]
        public float Width
        {
            get => SelfTransform.rect.width;
            set
            {
                var clampValue = Mathf.Clamp(value, minimalSize.Value.x, float.MaxValue);
                SelfTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, clampValue);
                isMinimized = false;
                isMaximized = false;

                gevtRestored?.Invoke(this);
                evtRestored.Invoke();
            }
        }

        /// <summary>
        /// Gets or set the height of the window
        /// </summary>
        /// <remarks>
        /// This respects the minimal size y value
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "<Pending>")]
        public float Height
        {
            get => SelfTransform.rect.height;
            set
            {
                var clampValue = Mathf.Clamp(value, minimalSize.Value.y, float.MaxValue);
                SelfTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, clampValue);
                isMinimized = false;
                isMaximized = false;

                gevtRestored?.Invoke(this);
                evtRestored.Invoke();
            }
        }

        /// <summary>
        /// Gets or sets the size of the window
        /// </summary>
        /// <remarks>
        /// This respects the minimal size x and y values
        /// </remarks>
        public Vector2 Size
        {
            get => SelfTransform.rect.size;
            set
            {
                Width = value.x;
                Height = value.y;
            }
        }

        public bool IsMaximized
        {
            get => isMaximized;
            set
            {
                if (value)
                    Maximize();
                else if (isMaximized)
                    Restore();
            }
        }

        public bool IsMinimized
        {
            get => isMinimized;
            set
            {
                if (value)
                    Minimize();
                else if (isMaximized)
                    Restore();
            }
        }

        private Vector2 preOperationSize;
        private Vector2 preOperationPosition;
        private GameObject previousSelected;
        private readonly Vector2 middle = new Vector2(0.5f, 0.5f);
        private bool isMaximized = false;
        private bool isMinimized = false;


        /// <summary>
        /// Sets the position and size of the window
        /// </summary>
        /// <remarks>
        /// This respects the minimal size and clamp to parent settings.
        /// </remarks>
        /// <param name="position">The position in local space to set the window</param>
        /// <param name="size">The size to set to the window</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>X represents width, if value 0 then width could not be set to the desired size, this is usually due clamping or minimal size restrictions</item>
        /// <item>Y represents height, if value 0 then height could not be set to the desired size, this is usually due to clamping or minimal size restrictions</item>
        /// <item>Z represents position, if value 0 then the position of the window could not be set to the desired value, this is usually due to clamping or minimal size restrictions</item>
        /// </list>
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "<Pending>")]
        public Vector3Int SetTransfrom(Vector3 position, Vector2 size)
        {
            var parent = SelfTransform.parent.GetComponent<RectTransform>();

            var changeApplied = new Vector3Int(0, 0, 0);

            if (size.x > minimalSize.Value.x)
            {
                if (!clampToParent.Value || (size.x / 2f + position.x <= parent.rect.width / 2f
                    && -size.x / 2f + position.x >= -parent.rect.width / 2f))
                {
                    if (size.x != SelfTransform.rect.width)
                        changeApplied.x = 1;
                    SelfTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
                }
            }
            else
            {
                if (minimalSize.Value.x != SelfTransform.rect.width)
                    changeApplied.x = 1;
                SelfTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minimalSize.Value.x);
            }

            if (size.y > minimalSize.Value.y)
            {
                if (!clampToParent.Value || (size.y / 2f + position.y <= parent.rect.height / 2f
                    && -size.y / 2f + position.y >= -parent.rect.height / 2f))
                {
                    if (size.y != SelfTransform.rect.height)
                        changeApplied.y = 1;
                    SelfTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
                }
            }
            else
            {
                if (minimalSize.Value.y != SelfTransform.rect.height)
                    changeApplied.y = 1;
                SelfTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minimalSize.Value.y);
            }

            if (changeApplied.x < 1)
                position.x = SelfTransform.localPosition.x;
            if (changeApplied.y < 1)
                position.y = SelfTransform.localPosition.y;

            if (Vector3.Distance(SelfTransform.localPosition, position) > 1)
                changeApplied.z = 1;

            SelfTransform.localPosition = position;

            if (changeApplied.x > 0
                || changeApplied.y > 0
                || changeApplied.z > 0)
            {
                isMinimized = false;
                isMaximized = false;

                gevtRestored?.Invoke(this);
                evtRestored.Invoke();
            }

            return changeApplied;
        }
        /// <summary>
        /// Updates the window's position and size to match the indicated transform
        /// </summary>
        /// <remarks>
        /// This respects the minimal size and clamp to parent settings
        /// </remarks>
        /// <param name="window"></param>
        /// <param name="transform"></param>
        public void SetTransfrom(RectTransform transform)
        {
            if (transform == null)
                return;

            var targetSize = new Vector2(transform.rect.width, transform.rect.height);
            Vector3 pivotDelta = transform.pivot - SelfTransform.pivot;
            pivotDelta.x *= targetSize.x;
            pivotDelta.y *= targetSize.y;

            SelfTransform.position = transform.position;
            SelfTransform.localPosition -= pivotDelta;

            Size = transform.rect.size;
            Move(SelfTransform.localPosition);
        }
        /// <summary>
        /// Sets this window as the focused window
        /// </summary>
        public void Focus() => API.Windows.Focused = this;
        /// <summary>
        /// Moves this window to this position
        /// </summary>
        /// <param name="position"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "<Pending>")]
        public void Move(Vector2 position)
        {
            if (clampToParent.Value && SelfTransform.parent != null)
                position = API.Windows.ClampPosition(this, position);

            if (position != (Vector2)SelfTransform.localPosition)
            {
                SelfTransform.localPosition = position;

                gevtRestored?.Invoke(this);
                evtRestored.Invoke();
            }
        }
        /// <summary>
        /// Expand the window to fill its parent rect
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "<Pending>")]
        public void Maximize()
        {
            preOperationPosition = Position;
            preOperationSize = Size;
            var parent = SelfTransform.parent.GetComponent<RectTransform>();
            SetTransfrom(parent);

            isMaximized = true;
            isMinimized = false;

            gevtMaximized?.Invoke(this);
            evtMaximized.Invoke();
        }
        /// <summary>
        /// Shrink the window to its minimal size
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "<Pending>")]
        public void Minimize()
        {
            preOperationPosition = Position;
            preOperationSize = Size;
            
            SelfTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minimalSize.Value.x);
            SelfTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minimalSize.Value.y);

            isMaximized = false;
            isMinimized = true;

            gevtMinimized?.Invoke(this);
            evtMinimized.Invoke();
        }
        /// <summary>
        /// Restore the window to the pre min or max size
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "<Pending>")]
        public void Restore()
        {
            if (isMinimized || isMaximized)
            {
                Size = preOperationSize;
                Position = preOperationPosition;

                isMinimized = false;
                isMaximized = false;

                gevtRestored?.Invoke(this);
                evtRestored.Invoke();
            }
        }
        /// <summary>
        /// Adjust the position of the window to a common snap location
        /// </summary>
        /// <param name="location"></param>
        public void SnapTo(WindowSnapToLocation location)
        {
            var parent = SelfTransform.parent.GetComponent<RectTransform>();

            switch(location)
            {
                case WindowSnapToLocation.BottomMiddle:
                    Position = new Vector2(0.5f, -parent.rect.height);
                    break;
                case WindowSnapToLocation.Centre:
                    Position = new Vector2(0.5f, 0.5f);
                    break;
                case WindowSnapToLocation.LeftMiddle:
                    Position = new Vector2(-parent.rect.width, 0.5f);
                    break;
                case WindowSnapToLocation.LowerLeft:
                    Position = new Vector2(-parent.rect.width, -parent.rect.height);
                    break;
                case WindowSnapToLocation.LowerRight:
                    Position = new Vector2(parent.rect.width, -parent.rect.height);
                    break;
                case WindowSnapToLocation.RightMiddle:
                    Position = new Vector2(parent.rect.width, 0.5f);
                    break;
                case WindowSnapToLocation.TopMiddle:
                    Position = new Vector2(0.5f, parent.rect.height);
                    break;
                case WindowSnapToLocation.UpperLeft:
                    Position = new Vector2(-parent.rect.width, parent.rect.height);
                    break;
                case WindowSnapToLocation.UpperRight:
                    Position = new Vector2(parent.rect.width, parent.rect.height);
                    break;
                case WindowSnapToLocation.LeftEdge:
                    Position = new Vector2(Position.x, 0.5f);
                    Size = new Vector2(Size.x, parent.rect.height);
                    break;
                case WindowSnapToLocation.RightEdge:
                    Position = new Vector2(Position.x, 0.5f);
                    Size = new Vector2(Size.x, parent.rect.height);
                    break;
                case WindowSnapToLocation.TopEdge:
                    Position = new Vector2(0.5f, Position.y);
                    Size = new Vector2(parent.rect.width, Size.y);
                    break;
                case WindowSnapToLocation.BottomEdge:
                    Position = new Vector2(0.5f, Position.y);
                    Size = new Vector2(parent.rect.width, Size.y);
                    break;
                case WindowSnapToLocation.Height:
                    Position = new Vector2(Position.x, 0.5f);
                    Size = new Vector2(Size.x, parent.rect.height);
                    break;
                case WindowSnapToLocation.Width:
                    Position = new Vector2(0.5f, Position.y);
                    Size = new Vector2(parent.rect.width, Size.y);
                    break;
            }
        }

        public void OnPointerDown(PointerEventData eventData) => API.Windows.Focused = this;

        private void Update()
        {
            if (SelfTransform.anchorMin != middle)
                SelfTransform.anchorMin = middle;

            if (SelfTransform.anchorMax != middle)
                SelfTransform.anchorMax = middle;

            if (SelfTransform.pivot != middle)
                SelfTransform.pivot = middle;
        }

        private void LateUpdate()
        {
            var currentSelected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            if (currentSelected != previousSelected)
            {
                previousSelected = currentSelected;
                if (previousSelected != null && previousSelected.transform.IsChildOf(SelfTransform))
                {
                    API.Windows.Focused = this;
                }
                else if (previousSelected != null && API.Windows.Focused == this)
                {
                    API.Windows.Focused = null;
                }
            }

            if(alwaysOnTop.Value)
            {
                SelfTransform.SetAsLastSibling();
            }    
        }

        private void Start()
        {
            API.Windows.RegisterWindow(this);

            API.Windows.EventWindowFocusChanged.AddListener(HandleFocusedChanged);

            SelfTransform.anchorMin = middle;
            SelfTransform.anchorMax = middle;
            SelfTransform.pivot = middle;
        }

        private void OnDestroy()
        {
            API.Windows.RemoveWindow(this);

            API.Windows.EventWindowFocusChanged.RemoveListener(HandleFocusedChanged);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "It is completly legitimate that a member be null, in line null propigation is a valid concept Unity ... update this rule")]
        private void HandleFocusedChanged(WindowFocusChangeEventData data)
        {
            if (data.previous == this)
            {
                evtLostFocus.Invoke();
                gevtLostFocus?.Invoke(this);
            }
            else if (data.current == this)
            {
                evtFocused.Invoke();
                gevtFocused?.Invoke(this);
            }
        }
    }
}


#endif