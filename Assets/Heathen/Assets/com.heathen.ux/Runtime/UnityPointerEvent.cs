#if HE_SYSCORE

using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace HeathenEngineering.UX
{
    [Serializable]
    public class UnityPointerEvent : UnityEvent<PointerEventData>
    { }
}

#endif