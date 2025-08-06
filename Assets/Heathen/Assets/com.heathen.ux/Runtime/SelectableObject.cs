#if HE_SYSCORE

using UnityEngine.Events;

namespace HeathenEngineering.UX
{
    public class SelectableObject : HeathenBehaviour
    {
        public bool IsSelected
        {
            get => API.Selection.Contains(this);
            set
            {
                if (value)
                    API.Selection.Add(this);
                else
                    API.Selection.Remove(this);
            }
        }

        public UnityEvent evtSelectionChanged;

        private void Start()
        {
            API.Selection.RegisterSelectable(this);
        }

        private void OnDestroy()
        {
            API.Selection.RemoveSelectable(this);
        }
    }
}

#endif
