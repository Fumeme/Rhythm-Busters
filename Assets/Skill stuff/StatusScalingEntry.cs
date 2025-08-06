using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewStatusEffectScaling", menuName = "Skill System/Status Effect Scaling", order = 1)]
public class StatusEffectScaling : ScriptableObject
{
    public List<SkillEffect.StatusScalingEntry> statusScalings;
}