using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Status Effect", menuName = "Skill System/Status Effect Definition")]
[Serializable]
public class StatusEffectDefinition : ScriptableObject
{
    public string StatusName = "Arson";
    public string Description = "also Arson";
    public StatusEffect.EffectSign sign = StatusEffect.EffectSign.NoSign;
    public int TierCount = 3;
    public int Duration = 15;

}