#if HE_SYSCORE

using HeathenEngineering.Events;
using System;
using UnityEngine.Events;

namespace HeathenEngineering.UX
{
    [Serializable]
    public class UnityDropHandledEventData : UnityEvent<EventData<DragAndDropItemChangeData>> { }
}

#endif