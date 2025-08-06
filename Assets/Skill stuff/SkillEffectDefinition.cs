using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill Effect", menuName = "Skill System/Skill Effect Definition")]
[Serializable]
public class SkillEffectDefinition : ScriptableObject
{
    public string SkillEffectName = "slap damage";
    public string Description = "damage for 50%";
    [Min(0)] public int duration = 5, power = 50, BonusMult = 1;

    public SkillEffect.Targeting targetting = SkillEffect.Targeting.EnemyFirst;
    public List<CharacterStats.ClassType> TargettingFilter;

    public SkillEffect.EffectType effectType = SkillEffect.EffectType.Damage;
    public StatusEffect.EffectSign sign = StatusEffect.EffectSign.Positive;
    public List<StatModifier> StatusesToApply = new();
    public StatusEffectScaling StatusEffectScaling = null;
    public Character.DamageSourceType requiredDamageSource = Character.DamageSourceType.None;
}