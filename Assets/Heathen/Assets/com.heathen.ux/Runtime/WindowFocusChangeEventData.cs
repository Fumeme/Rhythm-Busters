#if HE_SYSCORE

using System;

namespace HeathenEngineering.UX
{
    [Serializable]
    public struct WindowFocusChangeEventData
    {
        public Window previous;
        public Window current;
    }
}


#endif