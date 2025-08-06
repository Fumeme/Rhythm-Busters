#if HE_SYSCORE

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.UX
{
    public class SelectionManager : MonoBehaviour, ICollection<SelectableObject>
    {
        public int Count => API.Selection.selected.Count;

        public bool IsReadOnly => true;

        public API.Selection.ChangeEvent evtSelectionChanged;

        private void OnEnable()
        {
            API.Selection.EventSelectionChanged.AddListener(evtSelectionChanged.Invoke);
        }

        private void OnDisable()
        {
            API.Selection.EventSelectionChanged.RemoveListener(evtSelectionChanged.Invoke);
        }

        public void Add(SelectableObject selectable) => API.Selection.Add(selectable);

        public void AddRange(IEnumerable<SelectableObject> selectables) => API.Selection.AddRange(selectables);

        public bool Remove(SelectableObject selectable) => API.Selection.Remove(selectable);

        public int RemoveAll(IEnumerable<SelectableObject> selectables) => API.Selection.RemoveAll(selectables);

        public void Clear() => API.Selection.Clear();

        public bool Contains(SelectableObject item) => API.Selection.Contains(item);

        public void CopyTo(SelectableObject[] array, int arrayIndex) => API.Selection.selected.CopyTo(array, arrayIndex);

        public IEnumerator<SelectableObject> GetEnumerator() => API.Selection.selected.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => API.Selection.selected.GetEnumerator();
    }
}

#endif
