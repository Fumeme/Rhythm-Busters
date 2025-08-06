#if HE_SYSCORE

using System;

namespace HeathenEngineering.UX
{
    [Serializable]
    public enum WindowSnapToLocation
    {
        Centre,
        LeftMiddle,
        RightMiddle,
        TopMiddle,
        BottomMiddle,
        LowerLeft,
        LowerRight,
        UpperLeft,
        UpperRight,
        LeftEdge,
        RightEdge,
        TopEdge,
        BottomEdge,
        Height,
        Width,
    }
}


#endif