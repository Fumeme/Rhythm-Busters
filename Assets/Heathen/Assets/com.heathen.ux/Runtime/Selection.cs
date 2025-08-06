#if HE_SYSCORE

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.UX.API
{
    public static class Selection
    {
        [Serializable]
        public struct ChangeData
        {
            public SelectableObject[] Added { get; private set; }
            public SelectableObject[] Removed { get; private set; }

            public ChangeData(IEnumerable<SelectableObject> added, IEnumerable<SelectableObject> removed)
            {
                if (added != null)
                    Added = new List<SelectableObject>(added).ToArray();
                else
                    Added = new SelectableObject[0];

                if (removed != null)
                    Removed = new List<SelectableObject>(removed).ToArray();
                else
                    Removed = new SelectableObject[0];
            }
        }

        [Serializable]
        public class ChangeEvent : UnityEvent<ChangeData> { }

        public static ChangeEvent EventSelectionChanged => evtSelectionChanged;
        private static ChangeEvent evtSelectionChanged = new ChangeEvent();

        /// <summary>
        /// Never modify this or you will break the Selectable System
        /// </summary>
        public static readonly List<SelectableObject> selected = new List<SelectableObject>();
        /// <summary>
        /// Never modify this or you will break the Selectable System
        /// </summary>
        public static readonly List<SelectableObject> selectables = new List<SelectableObject>();

        public static int SelectedCount => selected.Count;
        public static int SelectableCount => selectables.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            selected.Clear();
            selectables.Clear();
            evtSelectionChanged = new ChangeEvent();
        }

#pragma warning disable UNT0008 // Null propagation on Unity objects
        public static void Add(SelectableObject item)
        {
            if (!selected.Contains(item))
            {
                selected.Add(item);

                evtSelectionChanged.Invoke(new ChangeData(new SelectableObject[] { item }, null));
                item?.evtSelectionChanged.Invoke();
            }
        }
        public static void AddRange(params SelectableObject[] items)
        {
            var added = new List<SelectableObject>();
            foreach (var item in items)
            {
                if (!selected.Contains(item))
                {
                    selected.Add(item);
                    added.Add(item);
                }
            }

            if (added.Count > 0)
                evtSelectionChanged?.Invoke(new ChangeData(added.ToArray(), null));

            foreach (var item in items)
                item?.evtSelectionChanged.Invoke();
        }
        public static void AddRange(IEnumerable<SelectableObject> items)
        {
            var added = new List<SelectableObject>();
            foreach (var item in items)
            {
                if (!selected.Contains(item))
                {
                    selected.Add(item);
                    added.Add(item);
                }
            }

            if (added.Count > 0)
                evtSelectionChanged?.Invoke(new ChangeData(added.ToArray(), null));

            foreach (var item in items)
                item?.evtSelectionChanged.Invoke();
        }
        public static bool Remove(SelectableObject item)
        {
            if (selected.Remove(item))
            {
                evtSelectionChanged?.Invoke(new ChangeData(null, new SelectableObject[] { item }));
                item?.evtSelectionChanged.Invoke();
                return true;
            }
            else
                return false;
        }
        public static int RemoveAll(IEnumerable<SelectableObject> items)
        {
            var removed = new List<SelectableObject>();
            foreach (var item in items)
            {
                if (selected.Remove(item))
                    removed.Add(item);
            }

            if (removed.Count > 0)
            {
                evtSelectionChanged?.Invoke(new ChangeData(null, removed.ToArray()));

                foreach (var item in removed)
                    item?.evtSelectionChanged.Invoke();

                return removed.Count;
            }
            else
                return 0;
        }
        public static int RemoveAll(ScriptableObject tag) => RemoveAll(selected.Where(predicate => predicate.ContainsScriptableTag(tag)));
        public static int RemoveAll(IEnumerable<ScriptableObject> tags) => RemoveAll(selected.Where(predicate => predicate.ContainsScriptableTags(tags)));
        public static int RemoveAll(string unityTag) => RemoveAll(selected.Where(predicate => predicate.tag == unityTag));
        public static int RemoveAll(Func<SelectableObject, bool> predicate) => RemoveAll(selected.Where(predicate));
        public static void Clear()
        {
            if (selected.Count > 0)
            {
                var removed = selected.ToArray();
                selected.Clear();
                evtSelectionChanged?.Invoke(new ChangeData(null, removed));

                foreach (var item in removed)
                    item?.evtSelectionChanged.Invoke();
            }
        }
        public static bool Contains(SelectableObject item) => selected.Contains(item);
        public static bool Contains(ScriptableObject tag) => selected.Any(predicate => predicate.ContainsScriptableTag(tag));
        public static bool Contains(IEnumerable<ScriptableObject> tags) => selected.Any(predicate => predicate.ContainsScriptableTags(tags));
        public static bool Contains(string unityTag) => selected.Any(predicate => predicate.tag == unityTag);
        public static SelectableObject[] GetSelected() => selected.ToArray();
        public static SelectableObject GetFirstSelected(params ScriptableObject[] tags)
        {
            if (tags.Length == 1)
                return selected.FirstOrDefault(predicate => predicate.ContainsScriptableTag(tags[0]));
            else if (tags.Length > 1)
                return selected.FirstOrDefault(predicate => predicate.ContainsScriptableTags(tags));
            else
                return null;
        }
        public static SelectableObject GetFirstSelected(ScriptableObject tag) => selected.FirstOrDefault(predicate => predicate.ContainsScriptableTag(tag));
        public static SelectableObject GetFirstSelected(IEnumerable<ScriptableObject> tags) => selected.FirstOrDefault(predicate => predicate.ContainsScriptableTags(tags));
        public static SelectableObject GetFirstSelected(string unityTag) => selected.FirstOrDefault(predicate => predicate.tag == unityTag);
        public static SelectableObject GetFirstSelected(Func<SelectableObject, bool> predicate) => selected.FirstOrDefault(predicate);
        public static SelectableObject[] GetAllSelected(params ScriptableObject[] tags)
        {
            if (tags.Length == 1)
                return selected.Where(predicate => predicate.ContainsScriptableTag(tags[0])).ToArray();
            else if (tags.Length > 1)
                return selected.Where(predicate => predicate.ContainsScriptableTags(tags)).ToArray();
            else
                return new SelectableObject[0];
        }
        public static SelectableObject[] GetAllSelected(ScriptableObject tag) => selected.Where(predicate => predicate.ContainsScriptableTag(tag)).ToArray();
        public static SelectableObject[] GetAllSelected(IEnumerable<ScriptableObject> tags) => selected.Where(predicate => predicate.ContainsScriptableTags(tags)).ToArray();
        public static SelectableObject[] GetAllSelected(string unityTag) => selected.Where(predicate => predicate.tag == unityTag).ToArray();
        public static SelectableObject[] GetAllSelected(Func<SelectableObject, bool> predicate) => selected.Where(predicate).ToArray();
#pragma warning restore UNT0008 // Null propagation on Unity objects

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void RegisterSelectable(SelectableObject selectable)
        {
            selectables.Add(selectable);
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static bool RemoveSelectable(SelectableObject selectable)
        {
            return selectables.Remove(selectable);
        }

        /// <summary>
        /// Gets an array of the currently known selectable objects rather they are selected or not
        /// </summary>
        /// <returns></returns>
        public static SelectableObject[] GetSelectables() => selectables.ToArray();
        public static SelectableObject Find(params ScriptableObject[] tags)
        {
            if (tags.Length == 1)
                return selectables.FirstOrDefault(predicate => predicate.ContainsScriptableTag(tags[0]));
            else if (tags.Length > 1)
                return selectables.FirstOrDefault(predicate => predicate.ContainsScriptableTags(tags));
            else
                return null;
        }
        /// <summary>
        /// Search all selectable objects
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static SelectableObject Find(ScriptableObject tag) => selectables.FirstOrDefault(predicate => predicate.ContainsScriptableTag(tag));
        /// <summary>
        /// Search all selectable objects
        /// </summary>
        public static SelectableObject Find(IEnumerable<ScriptableObject> tags) => selectables.FirstOrDefault(predicate => predicate.ContainsScriptableTags(tags));
        /// <summary>
        /// Search all selectable objects
        /// </summary>
        public static SelectableObject Find(string unityTag) => selectables.FirstOrDefault(predicate => predicate.tag == unityTag);
        /// <summary>
        /// Search all selectable objects
        /// </summary>
        public static SelectableObject Find(Func<SelectableObject, bool> predicate) => selectables.FirstOrDefault(predicate);
        
        public static SelectableObject[] FindAll(params ScriptableObject[] tags)
        {
            if (tags.Length == 1)
                return selectables.Where(predicate => predicate.ContainsScriptableTag(tags[0])).ToArray();
            else if (tags.Length > 1)
                return selectables.Where(predicate => predicate.ContainsScriptableTags(tags)).ToArray();
            else
                return new SelectableObject[0];
        }
        /// <summary>
        /// Search all selectable objects
        /// </summary>
        public static SelectableObject[] FindAll(ScriptableObject tag) => selectables.Where(predicate => predicate.ContainsScriptableTag(tag)).ToArray();
        /// <summary>
        /// Search all selectable objects
        /// </summary>
        public static SelectableObject[] FindAll(IEnumerable<ScriptableObject> tags) => selectables.Where(predicate => predicate.ContainsScriptableTags(tags)).ToArray();
        /// <summary>
        /// Search all selectable objects
        /// </summary>
        public static SelectableObject[] FindAll(string unityTag) => selectables.Where(predicate => predicate.tag == unityTag).ToArray();
        /// <summary>
        /// Search all selectable objects
        /// </summary>
        public static SelectableObject[] FindAll(Func<SelectableObject, bool> predicate) => selectables.Where(predicate).ToArray();
    }
}


#endif