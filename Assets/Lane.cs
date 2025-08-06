using System;
using UnityEngine;
using static LanesManager;

[CreateAssetMenu(menuName = "Lane")]
[Serializable]
public class Lane : ScriptableObject
{
    public LaneID laneID;
    public enum MainStat
    {
        Arcane,
        Might
    }    
    public MainStat mainStat = MainStat.Might;


    // Returns the ID of the lane
    public LaneID GetID() { return laneID; }
    public MainStat GetMainStat() { return mainStat; }


}
