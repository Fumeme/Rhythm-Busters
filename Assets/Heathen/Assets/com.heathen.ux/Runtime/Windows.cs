#if HE_SYSCORE

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
#endif

namespace HeathenEngineering.UX.API
{
    public static class Windows
    {
        /// <summary>
        /// Get or set the focused window
        /// </summary>
        public static Window Focused
        {
            get => focused;
            set
            {
                if(value != focused)
                {
                    var old = focused;
                    focused = value;
                    if (focused != null)
                        focused.SelfTransform.SetAsLastSibling();
                    eventFocusChanged.Invoke(new WindowFocusChangeEventData { current = focused, previous = old });
                }
            }
        }
                
        /// <summary>
        /// Occurs when the focused window changes
        /// </summary>
        public static WindowFocusChangeEvent EventWindowFocusChanged
        {
            get => eventFocusChanged;
        }

        private static Window focused;
        private static WindowFocusChangeEvent eventFocusChanged = new WindowFocusChangeEvent();

        private static readonly List<Window> availableWindows = new List<Window>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            availableWindows.Clear();
            eventFocusChanged = new WindowFocusChangeEvent();
            focused = null;
        }

        /// <summary>
        /// Updates the window's position and size to match the indicated transform
        /// </summary>
        /// <remarks>
        /// This respects the minimal size and clamp to parent settings
        /// </remarks>
        /// <param name="window"></param>
        /// <param name="transform"></param>
        public static void SetTransfrom(Window window, RectTransform transform) => window.SetTransfrom(transform);
        /// <summary>
        /// Sets the position and size of the window
        /// </summary>
        /// <remarks>
        /// This respects the minimal size and clamp to parent settings
        /// </remarks>
        /// <param name="window">The window to update</param>
        /// <param name="position">The position in local space to set the window</param>
        /// <param name="size">The size to set to the window</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>X represents width, if value 0 then width could not be set to the desired size, this is usually due clamping or minimal size restrictions</item>
        /// <item>Y represents height, if value 0 then height could not be set to the desired size, this is usually due to clamping or minimal size restrictions</item>
        /// <item>Z represents position, if value 0 then the position of the window could not be set to the desired value, this is usually due to clamping or minimal size restrictions</item>
        /// </list>
        /// </returns>
        public static Vector3Int SetTransfrom(Window window, Vector3 position, Vector2 size) => window.SetTransfrom(position, size);
        /// <summary>
        /// Sets the window to fill the parent container
        /// </summary>
        /// <param name="window"></param>
        public static void Maximize(Window window) => window.Maximize();
        /// <summary>
        /// Sets the window to its minimal size
        /// </summary>
        /// <param name="window"></param>
        public static void Minimize(Window window) => window.Minimize();
        /// <summary>
        /// Restores the windows size and position what it was before the last minimize or maximize operation
        /// </summary>
        /// <param name="window"></param>
        public static void Restore(Window window) => window.Restore();
        /// <summary>
        /// Get the clamping bounds that the window can be positioned within its parent based on its current size
        /// </summary>
        /// <param name="window">The window to test</param>
        /// <returns>A <see cref="WindowClampingBounds"/> representing the range of valid positions for this window assuming it must fall within the bounds of its parent.</returns>
        public static WindowClampingBounds PositionBounds(Window window)
        {
            var bounds = new WindowClampingBounds();
            bounds.left = window.Width * window.SelfTransform.pivot.x;
            bounds.right = window.Width - bounds.left;
            bounds.bottom = window.Height * window.SelfTransform.pivot.y;
            bounds.top = window.Height - bounds.bottom;

            var parent = window.SelfTransform.parent.GetComponent<RectTransform>();
            bounds.left = -((parent.rect.width / 2f) - bounds.left);
            bounds.right = (parent.rect.width / 2f) - bounds.right;
            bounds.bottom = -((parent.rect.height / 2f) - bounds.bottom);
            bounds.top = (parent.rect.height / 2f) - bounds.top;
            return bounds;
        }
        /// <summary>
        /// Returns the clamped position for this window
        /// </summary>
        /// <remarks>
        /// The clamped position is a position which causes the whole window to fall within the parent rects bounds.
        /// </remarks>
        /// <param name="window">The window to clamp</param>
        /// <param name="position">The position for this window to be clamped</param>
        /// <returns>The resulting clamped position</returns>
        public static Vector2 ClampPosition(Window window, Vector2 position)
        {
            var boundPosition = position;
            var bounds = PositionBounds(window);
            boundPosition.x = Mathf.Clamp(boundPosition.x, bounds.left, bounds.right);
            boundPosition.y = Mathf.Clamp(boundPosition.y, bounds.bottom, bounds.top);

            return boundPosition;
        }
        /// <summary>
        /// Gets the collection of all windows currently registered to the system
        /// </summary>
        /// <returns></returns>
        public static Window[] GetAvailableWindows() => availableWindows.ToArray();
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void RegisterWindow(Window window) => availableWindows.Add(window);
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static bool RemoveWindow(Window window) => availableWindows.Remove(window);
        public static Window FindWindowWithTag(ScriptableObject tag) => availableWindows.FirstOrDefault(predicate => predicate.ContainsScriptableTag(tag));
        public static Window[] FindWindowsWithTag(ScriptableObject tag) => availableWindows.Where(predicate => predicate.ContainsScriptableTag(tag)).ToArray();
        public static Window FindWindowWithTags(IEnumerable<ScriptableObject> tags) => availableWindows.FirstOrDefault(predicate => predicate.ContainsScriptableTags(tags));
        public static Window[] FindWindowsWithTags(IEnumerable<ScriptableObject> tags) => availableWindows.Where(predicate => predicate.ContainsScriptableTags(tags)).ToArray();
        [System.Obsolete("Unity's tag system is neither efficent nor flexable, your are strongly encruaged to use System Core's scriptable tags. You can learn more at https://kb.heathenengineering.com/assets/system-core/scriptable-tags")]
        public static Window FindWindowWithUnityTag(string tag) => availableWindows.FirstOrDefault(predicate => predicate.tag == tag);
        [System.Obsolete("Unity's tag system is neither efficent nor flexable, your are strongly encruaged to use System Core's scriptable tags. You can learn more at https://kb.heathenengineering.com/assets/system-core/scriptable-tags")]
        public static Window[] FindWindowsWithUnityTag(string tag) => availableWindows.Where(predicate => predicate.tag == tag).ToArray();
    }
}


#endif