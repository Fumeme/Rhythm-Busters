#if HE_SYSCORE

using HeathenEngineering.Events;
using System;
using UnityEngine.EventSystems;

namespace HeathenEngineering.UX
{
    [Serializable]
    public class PointerGameEvent : GameEvent<PointerEventData>
    { }


}

#endif