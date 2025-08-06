#if HE_SYSCORE

using System;

namespace HeathenEngineering.UX
{
    [Serializable]
    public struct DragAndDropItemChangeData
    {
        public DragItem previousValue;
        public DragItem newValue;
    }
}


#endif